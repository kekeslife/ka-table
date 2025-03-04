using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace KaTableNet9Test
{
    /// <summary>
    /// 表格操作类
    /// </summary>
    /// <typeparam name="TEntity">实体类</typeparam>
    /// <typeparam name="TDto">业务类</typeparam>
    public class KaTable<TEntity, TDto> where TEntity : class, new() where TDto : class, new()
    {
        private static readonly Lang LangChinese = new()
        {
            TooManyRows = "有多笔数据",
            NoRecords = "无此记录或已变更",
            RecordChanged = "记录已变更",
            ExcelTitle = "Excel第{0}列标题不是({1})",
        };

        private static readonly Lang LangEnglish = new()
        {
            TooManyRows = "There are multiple records",
            NoRecords = "No such record or changed",
            RecordChanged = "Record changed",
            ExcelTitle = "Excel column {0} title is not ({1})",
        };

        /// <summary>
        /// 数组Contains的表达式树
        /// </summary>
        private readonly MethodInfo _containsMethod =
            ((MethodCallExpression)((Expression<Func<IEnumerable<object>, bool>>)(q =>
                q.Contains(null))).Body).Method;

        /// <summary>
        /// 字符串比较表达式树
        /// </summary>
        private readonly MethodInfo _compareMethod = typeof(string).GetMethod("Compare", [typeof(string), typeof(string)])!;


        /// <summary>
        /// 类属性缓存
        /// </summary>
        private readonly Dictionary<string, PropertyInfo> _entityProps = new();

        private readonly Dictionary<string, PropertyInfo> _dtoProps = new();
        private readonly List<PropertyInfo> _keyProps = [];

        /// <summary>
        /// Linq的Include集合
        /// </summary>
        private readonly List<Expression<Func<TEntity, object>>> _includeExps = [];

        /// <summary>
        /// JSON反序列化设置
        /// </summary>
        private readonly JsonSerializerOptions _jsonDesSetting = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

        /// <summary>
        /// JSON序列化设置
        /// </summary>
        private readonly JsonSerializerOptions _jsonSetting = new()
        {
            // 排除null
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // NullValueHandling = NullValueHandling.Ignore,

            // 不循环引用
            // ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,

            // 首字母小写
            // ContractResolver = new CamelCasePropertyNamesContractResolver(),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// JSON反序列化第一层
        /// </summary>
        private readonly JsonSerializerOptions _jsonSettingDepth1 = new()
        {
            Converters = { new MaxDepthConverter(1) },
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// JSON反序列化2层
        /// </summary>
        private readonly JsonSerializerOptions _jsonSettingDepth2 = new()
        {
            Converters = { new MaxDepthConverter(2) },
            PropertyNameCaseInsensitive = true
        };

        private DbContext Db { get; }

        private Lang LangLocale { get; }

        #region 属性

        /// <summary>
        /// 自定义Where
        /// </summary>
        public Expression<Func<TEntity, bool>>? CustomerWhere { get; set; }
        
        /// <summary>
        /// 自定义预设排序
        /// </summary>
        public  List<KaTableCustomSort<TEntity>>? CustomerOrders { get; set; }
        
        /// <summary>
        /// 自定义Select
        /// </summary>
        public Expression<Func<TEntity, TDto>>? CustomerSelect { get; set; }

        /// <summary>
        /// 自定义聚合,Total为总笔数,加入会加快，否则会产生两笔sum
        /// <example>
        /// <code>
        /// table.CustomerSummary = t => new
        /// {
        ///     // Total为查询出的总行数，务必添加，不然会执行两次汇总sql
        ///     Total = t.Count(),
        ///     // 取得最大的编号
        ///     MaxReqNo = t.Max(d => d.ReqNo)
        /// };
        /// </code>
        /// </example>
        /// </summary>
        public Expression<Func<IGrouping<int, TEntity>, object>>? CustomerSummary { get; set; }

        /// <summary>
        /// 读取数据后
        /// <remarks>
        /// 参数：
        /// <list type="bullet">
        ///   <item><description><c>IList&lt;TDto&gt;</c>: dto数据。</description></item>
        /// </list>
        /// </remarks>
        /// </summary>
        public Action<List<TDto>>? AfterLoadData { get; set; }

        /// <summary>
        /// 导出，在读取数据之前
        /// </summary>
        public Action<KaTableExportParameter>? BeforeExportLoadData { get; set; }

        /// <summary>
        /// 导出，在读取数据之后，产生Excel之前
        /// </summary>
        public Action<IList<TDto>>? BeforeExport { get; set; }

        /// <summary>
        /// 导出，产生Excel之后
        /// </summary>
        public Action<ExcelWorksheet, KaTableExportCondition[], int>? AfterExport { get; set; }

        /// <summary>
        /// 插入之前
        /// </summary>
        public Action<TDto>? BeforeInsert { get; set; }

        /// <summary>
        /// 插入之后
        /// </summary>
        public Action<TDto>? AfterInsert { get; set; }

        /// <summary>
        /// 更新数据之后，newRecord, oldRecord
        /// </summary>
        public Action<TEntity, TDto>? AfterUpdate { get; set; }

        /// <summary>
        /// 更新数据之前，newRecord, oldRecord
        /// </summary>
        public Action<TEntity, TDto>? BeforeUpdate { get; set; }

        /// <summary>
        /// 删除数据之前，dbEntity，oldDto
        /// </summary>
        public Action<TEntity, TDto>? BeforeDelete { get; set; }

        /// <summary>
        /// 删除数据之后，dbEntity，oldDto
        /// </summary>
        public Action<TEntity, TDto>? AfterDelete { get; set; }

        /// <summary>
        /// 下载模板之后
        /// </summary>
        public Action<ExcelWorksheet, KaTableImportCol[]>? AfterDownloadTemplate { get; set; }

        /// <summary>
        /// 导入文件之后
        /// <remarks>
        /// 该 Action 接受三个参数：
        /// <list type="bullet">
        ///   <item><description><c>entities</c>: 导入的第一层数据，返回给前端。</description></item>
        ///   <item><description><c>dtos</c>: 导入的所有数据。</description></item>
        ///   <item><description><c>importCols</c>: 导入文件的列配置。</description></item>
        /// </list>
        /// </remarks>
        /// </summary>
        public Action<IList<TEntity>, IList<TDto>, KaTableImportCol[]>? AfterImportFile { get; set; }

        /// <summary>
        /// 导入数据库之前
        /// </summary>
        public Action<TDto[]>? BeforeImport { get; set; }

        /// <summary>
        /// 导入数据库之后
        /// </summary>
        public Action<TDto[]>? AfterImport { get; set; }

        #endregion

        public KaTable(DbContext db, string lang = "cn")
        {
            Db = db;
            LangLocale = lang == "cn" ? LangChinese : LangEnglish;

            var entityProps = typeof(TEntity).GetProperties();
            foreach (var prop in entityProps)
            {
                if (prop.GetCustomAttribute<KeyAttribute>() != null)
                {
                    _keyProps.Add(prop);
                }

                _entityProps.Add(prop.Name, prop);
            }


            if (_keyProps.Count == 0)
            {
                throw new Exception("System:未设置主键");
            }

            var dtoProps = typeof(TDto).GetProperties();
            foreach (var prop in dtoProps)
            {
                _dtoProps.Add(prop.Name, prop);
            }
        }

        public IResult Action(HttpContext context)
        {
            IResult result;
            try
            {
                var act = context.Request.Form["actNo"];
                result = act.ToString().ToUpper() switch
                {
                    "SEARCH" => SearchAction(context),
                    "INSERT" => InsertAction(context),
                    "UPDATE" => UpdateAction(context),
                    "REMOVE" => DeleteAction(context),
                    "EXPORT" => ExportAction(context),
                    "IMPORT_FILE" => ImportFileAction(context),
                    "IMPORT" => ImportAction(context),
                    "DOWNLOAD_TEMPLATE" => DownloadTemplateAction(context),
                    _ => throw new Exception("System:未提供此操作")
                };
            }
            catch (ValidationException ex)
            {
                result = Results.Json(new KaTableResponse
                {
                    IsSuccess = false,
                    Message = ex.ValidationResult.ErrorMessage ?? ex.Message
                });
            }
            catch (DbUpdateException ex)
            {
                result = Results.Json(new KaTableResponse
                {
                    IsSuccess = false,
                    Message = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                result = Results.Json(new KaTableResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
            //finally
            //{
            //    context.Response.End();
            //}

            return result;
        }

        /// <summary>
        /// 添加Include子实体
        /// </summary>
        public KaTable<TEntity, TDto> Include(Expression<Func<TEntity, object>> exp)
        {
            _includeExps.Add(exp);
            return this;
        }

        #region action

        /// <summary>
        /// 查询
        /// </summary>
        private IResult SearchAction(HttpContext context)
        {
            var pars = context.Request.Form["searchPar"].ToString();
            if (string.IsNullOrEmpty(pars))
            {
                throw new Exception("System:没有参数searchPar");
            }

            var searchPar = JsonSerializer.Deserialize<KaTableSearchParameter>(pars, _jsonDesSetting);
            var result = SelectRecords(searchPar!);

            AfterLoadData?.Invoke(result.Records);

            return Results.Json(result);
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        private IResult ExportAction(HttpContext context)
        {
            var pars = context.Request.Form["exportPar"];
            if (string.IsNullOrEmpty(pars))
            {
                throw new Exception("System:exportPar");
            }

            var exportPar = JsonSerializer.Deserialize<KaTableExportParameter>(pars!, _jsonDesSetting);

            BeforeExportLoadData?.Invoke(exportPar!);
            var result = SelectRecords(exportPar!);
            BeforeExport?.Invoke(result.Records);

            var recordLen = result.Records.Count;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var excel = new ExcelPackage();
            var worksheet = excel.Workbook.Worksheets.Add(exportPar!.FileName);
            var colIndex = 1;
            foreach (var col in exportPar.Cols)
            {
                if (!string.IsNullOrWhiteSpace(col.Formula))
                {
                    // 公式
                    for (var rowIndex = 2; rowIndex < recordLen + 2; rowIndex++)
                    {
                        worksheet.Cells[rowIndex, colIndex].Formula = col.Formula
                            .Replace("$row$", rowIndex.ToString()).Replace("$col$", colIndex.ToString());
                    }
                }
                else
                {
                    var rowIndex = 2;
                    foreach (var record in result.Records)
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = GetPropertyValue(record, col.Key);
                        rowIndex++;
                    }
                }

                // 标题
                worksheet.Cells[1, colIndex].Value = col.Title;

                colIndex++;
            }

            AfterExport?.Invoke(worksheet, exportPar.Cols, recordLen);

            for (var i = 0; i < exportPar.Cols.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(exportPar.Cols[i].DateFormat))
                {
                    worksheet.Column(i + 1).Style.Numberformat.Format = exportPar.Cols[i].DateFormat;
                }
            }

            worksheet.Cells.AutoFitColumns();
            worksheet.Cells.Style.WrapText = true;

            var stream = excel.GetAsByteArray();

            return Results.File(stream);
        }

        /// <summary>
        /// 添加记录
        /// </summary>
        private IResult InsertAction(HttpContext context)
        {
            var data = context.Request.Form["record"];
            if (string.IsNullOrEmpty(data))
            {
                throw new Exception("System:没有参数record");
            }

            var dto = JsonSerializer.Deserialize<TDto>(data!, _jsonDesSetting);

            BeforeInsert?.Invoke(dto!);
            // BeforeInsert改变dto之后再产生entity
            var entity = GetFlatObject<TEntity>(JsonSerializer.Serialize(dto, _jsonSetting));

            using (var transaction = Db.Database.BeginTransaction())
            {
                try
                {
                    InsertRecord(entity);
                    AfterInsert?.Invoke(dto!);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }

            return Results.Json(new KaTableResponseRecord<TEntity> { IsSuccess = true, Record = entity });
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        private IResult UpdateAction(HttpContext context)
        {
            var oldRecord = context.Request.Form["oldRecord"];
            if (string.IsNullOrEmpty(oldRecord))
            {
                throw new Exception("System:没有参数oldRecord");
            }

            var newRecord = context.Request.Form["newRecord"];
            if (string.IsNullOrEmpty(newRecord))
            {
                throw new Exception("System:没有参数newRecord");
            }

            TEntity resultObj;

            var oldDto = JsonSerializer.Deserialize<TDto>(oldRecord!, _jsonDesSetting);
            var oldFlatDto = GetFlatObject<TDto>(oldRecord!);

            using (var transaction = Db.Database.BeginTransaction())
            {
                try
                {
                    // 从前端的oldRecord读取数据中的旧数据
                    var dbRecord = GetFlatEntityByJson(oldRecord!);
                    // 检查数据库中的数据时候和前端的oldRecord第一层相同
                    CheckOldRecord(dbRecord, oldFlatDto);

                    // 将前端的newRecord数据更新至dbRecord
                    SetRecordVal(dbRecord, GetFlatObject<Dictionary<string, JsonElement>>(newRecord!));

                    BeforeUpdate?.Invoke(dbRecord, oldDto!);
                    Db.SaveChanges();
                    AfterUpdate?.Invoke(dbRecord, oldDto!);

                    resultObj = dbRecord;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }

            return Results.Json(new KaTableResponseRecord<TEntity> { IsSuccess = true, Record = resultObj });
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        private IResult DeleteAction(HttpContext context)
        {
            var record = context.Request.Form["record"];
            if (string.IsNullOrEmpty(record))
            {
                throw new Exception("System:没有参数record");
            }

            var oldDto = JsonSerializer.Deserialize<TDto>(record!, _jsonDesSetting);
            var oldFlatDto = GetFlatObject<TDto>(record!);

            using (var transaction = Db.Database.BeginTransaction())
            {
                try
                {
                    // 从前端的oldRecord读取数据中的旧数据
                    var dbRecord = GetFlatEntityByJson(record!);
                    // 检查数据库中的数据时候和前端的oldRecord第一层相同
                    CheckOldRecord(dbRecord, oldFlatDto);

                    BeforeDelete?.Invoke(dbRecord, oldDto!);

                    Db.Set<TEntity>().Remove(dbRecord);
                    Db.SaveChanges();

                    AfterDelete?.Invoke(dbRecord, oldDto!);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }

            return Results.Json(new KaTableResponse { IsSuccess = true });
        }

        /// <summary>
        /// 导入文件
        /// </summary>
        private IResult ImportFileAction(HttpContext context)
        {
            var file = context.Request.Form.Files["file"];
            if (file == null)
            {
                throw new Exception("System:没有文件");
            }

            var colsStr = context.Request.Form["cols"];
            if (string.IsNullOrEmpty(colsStr))
            {
                throw new Exception("System:没有参数cols");
            }

            var cols = JsonSerializer.Deserialize<KaTableImportCol[]>(colsStr!, _jsonDesSetting);
            var result = GetRecordsByFile(cols!, file, out var dtoList);

            AfterImportFile?.Invoke(result.Records, dtoList, cols!);

            return Results.Json(result);
        }

        /// <summary>
        /// 导入数据库
        /// </summary>
        private IResult ImportAction(HttpContext context)
        {
            var data = context.Request.Form["records"];
            if (string.IsNullOrEmpty(data))
            {
                throw new Exception("System:没有参数records");
            }

            var dto = JsonSerializer.Deserialize<TDto[]>(data!, _jsonDesSetting);

            BeforeImport?.Invoke(dto!);
            var entity = GetFlatObject<TEntity[]>(JsonSerializer.Serialize(dto, _jsonSetting), _jsonSettingDepth2);

            using (var transaction = Db.Database.BeginTransaction())
            {
                try
                {
                    ImportRecord(entity);
                    AfterImport?.Invoke(dto!);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }

            return Results.Json(new KaTableResponse { IsSuccess = true });
        }

        /// <summary>
        /// 下载模板
        /// </summary>
        private IResult DownloadTemplateAction(HttpContext context)
        {
            var colsStr = context.Request.Form["cols"];
            if (string.IsNullOrEmpty(colsStr))
            {
                throw new Exception("System:没有参数cols");
            }

            var cols = JsonSerializer.Deserialize<KaTableImportCol[]>(colsStr!, _jsonDesSetting);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var excel = new ExcelPackage();
            var worksheet = excel.Workbook.Worksheets.Add("template");
            for (var i = 0; i < cols!.Length; i++)
            {
                var col = cols[i];
                if (!string.IsNullOrEmpty(col.Title))
                {
                    worksheet.Cells[1, i + 1].Value = col.Title;
                }
            }

            AfterDownloadTemplate?.Invoke(worksheet, cols);

            worksheet.Cells.AutoFitColumns();
            worksheet.Cells.Style.WrapText = true;

            var stream = excel.GetAsByteArray();
            return Results.File(stream);
        }

        #endregion

        #region 方法

        /// <summary>
        /// 查询数据库
        /// </summary>
        private KaTableSearchResponse<TDto> SelectRecords(KaTableSearchParameter searchPar)
        {
            var result = new KaTableSearchResponse<TDto>
            {
                IsSuccess = true,
            };

            //Db.Database.Connection.Open();
            var table = Db.Set<TEntity>() as IQueryable<TEntity>;

            // include
            foreach (var includeExp in _includeExps)
            {
                table = table.Include(includeExp);
            }

            table = table.AsNoTracking();

            // where
            var whereExpression = CreateWhereExpression(searchPar.WhereConditions);

            var data = table.Where(whereExpression);

            if (CustomerWhere != null)
            {
                data = data.Where(CustomerWhere);
            }

            // total
            var innerTot = true;
            if (CustomerSummary != null)
            {
                var summary = data.GroupBy(l => 0).Select(CustomerSummary).ToArray().FirstOrDefault();
                if (summary != null)
                {
                    var prop = summary.GetType().GetProperty("Total");
                    if (prop != null)
                    {
                        innerTot = false;
                        result.Total = Convert.ToInt32(prop.GetValue(summary));
                    }

                    result.Summary = summary;
                }
            }

            if (innerTot)
            {
                var total = data.Count();
                result.Total = total;
            }

            // order by
            var sortConditions = searchPar.SortConditions;
            if (sortConditions.Length == 0)
            {
                sortConditions = _keyProps.Select(k => new KaTableSortCondition
                {
                    Key = k.Name,
                    Order = "ascend"
                }).ToArray();
            }
            
            var queryExp = data.Expression;
            var i = 0;
            queryExp = sortConditions.Aggregate(queryExp,
                (current, condition) => CreateSortSubExpression(condition, current, i++));
            
            // custom sort
            if (CustomerOrders != null)
            {
                queryExp = CustomerOrders.Aggregate(queryExp,
                    (current, condition) => CreateCustomSortSubExpression(condition, current, i++));
            }
            

            data = data.Provider.CreateQuery<TEntity>(queryExp);

            // page
            if (searchPar.PageSize > 0)
            {
                data = data.Skip((searchPar.PageNum - 1) * searchPar.PageSize).Take(searchPar.PageSize);
            }

            // select
            if (CustomerSelect != null)
            {
                result.Records = data.Select(CustomerSelect).ToList();
            }
            else if (typeof(TDto) == typeof(TEntity))
            {
                result.Records = data.ToList() as List<TDto> ?? [];
            }
            else if (typeof(TEntity).IsSubclassOf(typeof(TDto)))
            {
                result.Records = data.ToList().ConvertAll(l => l as TDto)!;
            }
            else
            {
                result.Records = data.ToList().ConvertAll(Mapper<TEntity, TDto>);
            }

            return result;
        }

        /// <summary>
        /// 插入数据库
        /// </summary>
        private void InsertRecord(TEntity entity)
        {
            Db.Set<TEntity>().Add(entity);
            Db.SaveChanges();
        }

        /// <summary>
        /// 设置实体属性值
        /// </summary>
        private void SetRecordVal(TEntity entity, Dictionary<string, JsonElement> newRecord)
        {
            foreach (var col in newRecord)
            {
                var key = ToCamel(col.Key);

                if (!_dtoProps.ContainsKey(key))
                {
                    throw new Exception($"System:无此属性{key}");
                }
                
                if (!_entityProps.TryGetValue(key, out var entityProp))
                {
                    // throw new Exception($"System:无此属性{key}");
                    continue;
                }

                var colVal = GetJsonElementValue(col.Value, entityProp.PropertyType);

                entityProp.SetValue(entity, colVal);
            }
        }

        /// <summary>
        /// 取得第一层的实体，不包含子实体，需要优化null
        /// </summary>
        private TEntity GetFlatEntityByJson(string json)
        {
            var whereConditions = GetFlatObject<Dictionary<string, JsonElement>>(json)
                .Where(l => l.Value.ValueKind != JsonValueKind.Null).Select(l => new KaTableWhereCondition
                {
                    Key = l.Key,
                    Opt = "eq",
                    Val = l.Value,
                    Bool = "and"
                }).ToArray();

            var dbRecords = Db.Set<TEntity>().Where(CreateWhereExpression(whereConditions)).ToArray();

            return dbRecords.Length switch
            {
                > 1 => throw new Exception(LangLocale.TooManyRows),
                0 => throw new Exception(LangLocale.NoRecords),
                _ => dbRecords[0]
            };
        }

        /// <summary>
        /// 检查旧数据是否是数据库里面的，防止前端篡改
        /// </summary>
        /// <param name="dbEntity">数据库实体</param>
        /// <param name="oldDto">旧数据</param>
        private void CheckOldRecord(TEntity dbEntity, TDto oldDto)
        {
            TDto? newDto;

            if (CustomerSelect == null)
            {
                if (typeof(TDto) == typeof(TEntity))
                {
                    newDto = dbEntity as TDto;
                }
                else
                {
                    newDto = Mapper<TEntity, TDto>(dbEntity);
                }
            }
            else
            {
                newDto = new[] { dbEntity }.AsQueryable().Select(CustomerSelect).First();
            }


            var keys = _entityProps.Keys.Intersect(_dtoProps.Keys);

            foreach (var key in keys)
            {
                // var oldVal = _entityProps[key].GetValue(oldDto);
                // var newVal = _entityProps[key].GetValue(newDto);
                var oldVal = _dtoProps[key].GetValue(oldDto);
                var newVal = _dtoProps[key].GetValue(newDto);
                if (!Equals(oldVal, newVal))
                {
                    throw new Exception(LangLocale.RecordChanged);
                }
            }
        }

        /// <summary>
        /// 将excel文件转为实体
        /// </summary>
        private KaTableSearchResponse<TEntity> GetRecordsByFile(KaTableImportCol[] cols, IFormFile file, out List<TDto> dtoList)
        {
            dtoList = [];
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage(file.OpenReadStream());
            var sheet = package.Workbook.Worksheets[0];

            for (var i = 0; i < cols.Length; i++)
            {
                var col = cols[i];

                if (string.IsNullOrEmpty(col.Title)) continue;

                if (!string.Equals(sheet.Cells[1, i + 1].Value?.ToString(), col.Title,
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    throw new Exception(string.Format(LangLocale.ExcelTitle, i + 1, col.Title));
                }
            }

            var result = new List<TEntity>();
            var rowIndex = 2;
            while (true)
            {
                if (sheet.Cells[rowIndex, 1].Value == null) break;

                var dic = new Dictionary<string, object>();
                for (var colIndex = 0; colIndex < cols.Length; colIndex++)
                {
                    var col = cols[colIndex];


                    object? val = _dtoProps[ToCamel(col.Key)].PropertyType  switch
                    {
                        null=>null,
                        var t when t==typeof(int)||t==typeof(int?) => sheet.Cells[rowIndex, colIndex + 1].GetValue<int>(),
                        var t when t==typeof(long)||t==typeof(long?) => sheet.Cells[rowIndex, colIndex + 1].GetValue<long>(),
                        var t when t==typeof(double)||t==typeof(double?) => sheet.Cells[rowIndex, colIndex + 1].GetValue<double>(),
                        var t when t==typeof(float)||t==typeof(float?) => sheet.Cells[rowIndex, colIndex + 1].GetValue<float>(),
                        var t when t==typeof(decimal)||t==typeof(decimal?) => sheet.Cells[rowIndex, colIndex + 1].GetValue<decimal>(),
                        var t when t==typeof(DateTime)||t==typeof(DateTime?) => sheet.Cells[rowIndex, colIndex + 1].GetValue<DateTime>(),
                        var t when t==typeof(string) => sheet.Cells[rowIndex, colIndex + 1].GetValue<string>(),
                        var t when t==typeof(bool)||t==typeof(bool?) => sheet.Cells[rowIndex, colIndex + 1].GetValue<bool>(),
                        _=>null
                    };

                    if (val != null)
                    {
                        dic.Add(col.Key, val);
                    }
                    
                    // dic.Add(col.Key, sheet.Cells[rowIndex, colIndex + 1].Value);
                }

                var data = CreateObject(dic, out var obj);
                if (obj != null) result.Add(obj);
                if (data != null) dtoList.Add(data);

                rowIndex++;
            }

            return new KaTableSearchResponse<TEntity> { IsSuccess = true, Records = result, Total = result.Count };
        }

        /// <summary>
        /// 汇入数据库
        /// </summary>
        private void ImportRecord(TEntity[] entities)
        {
            Db.Set<TEntity>().AddRange(entities);
            Db.SaveChanges();
        }

        #endregion

        #region 表达式树

        // 建立排序表达式树
        private static Expression CreateSortSubExpression(KaTableSortCondition condition, in Expression oldExp,
            int sortIndex = 0)
        {
            if (condition.Order != "ascend" && condition.Order != "descend")
            {
                throw new Exception("System:排序参数错误ascend、descend");
            }
            var table = Expression.Parameter(typeof(TEntity), "t");
            var member = CreateExpressionProperty(condition.Key, table);
            if (member == null) return oldExp;
            string ordMthStr;
            if (condition.Order == "ascend")
            {
                ordMthStr = sortIndex == 0 ? "OrderBy" : "ThenBy";
            }
            else
            {
                ordMthStr = sortIndex == 0 ? "OrderByDescending" : "ThenByDescending";
            }

            var ordLamb = Expression.Lambda(member, table);

            return Expression.Call(typeof(Queryable), ordMthStr,
                [typeof(TEntity), ((PropertyInfo)member.Member).PropertyType], oldExp, ordLamb);
        }

        private static MethodCallExpression CreateCustomSortSubExpression(KaTableCustomSort<TEntity> condition, in Expression oldExp,
            int sortIndex = 0)
        {
            if (condition.Order != "ascend" && condition.Order != "descend")
            {
                throw new Exception("System:排序参数错误ascend、descend");
            }
            string ordMthStr;
            if (condition.Order == "ascend")
            {
                ordMthStr = sortIndex == 0 ? "OrderBy" : "ThenBy";
            }
            else
            {
                ordMthStr = sortIndex == 0 ? "OrderByDescending" : "ThenByDescending";
            }
            return Expression.Call(typeof(Queryable), ordMthStr,
                [typeof(TEntity), typeof(object)], oldExp, condition.SortExpression);
        }

        // 将反序列化where条件为表达式树
        private Expression<Func<TEntity, bool>> CreateWhereExpression(KaTableWhereCondition[] conditions)
        {
            var table = Expression.Parameter(typeof(TEntity), "t");

            Expression? exp = null;
            var index = 0;

            foreach (var condition in conditions)
            {
                if (string.IsNullOrEmpty(condition.Bool))
                {
                    condition.Bool = "and";
                }

                var subExpression = CreateWhereSubExpression(condition, table);

                if (++index == 1)
                {
                    exp = subExpression;
                    continue;
                }

                Debug.Assert(exp != null, nameof(exp) + " != null");
                exp = condition.Bool == "and"
                    ? Expression.AndAlso(exp, CreateWhereSubExpression(condition, table))
                    : Expression.OrElse(exp, CreateWhereSubExpression(condition, table));
            }

            return Expression.Lambda<Func<TEntity, bool>>(exp ?? Expression.Constant(true), table);
        }

        private Expression CreateWhereSubExpression(KaTableWhereCondition condition, ParameterExpression table)
        {
            if (condition.Children != null) return CreateWhereSubGroupExpression(condition, table);

            if (string.IsNullOrEmpty(condition.Key))
            {
                throw new ArgumentException("System:Key不能为空", condition.Key);
            }

            if (string.IsNullOrEmpty(condition.Opt))
            {
                throw new ArgumentException("System:Opt不能为空", condition.Opt);
            }

            var property = CreateExpressionProperty(condition.Key, table);
            if (property == null) throw new ArgumentException("System:无此属性", condition.Key); // return Expression.Constant(true);
            var propertyType = property.Type;

            var isnullType = IsNullable((PropertyInfo)property.Member);
            // 可空类型的原始类
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            Expression conditionValExp;

            if (condition.Opt is "nu" or "nnu")
            {
                conditionValExp = Expression.Constant(null);
            }
            else
            {
                var conditionVal = GetJsonElementValue(condition.Val, propertyType);
                if (conditionVal!.GetType().IsArray)
                {
                    conditionValExp = Expression.Constant(conditionVal);
                }
                else
                {
                    conditionValExp = isnullType
                        ? Expression.Convert(Expression.Constant(conditionVal), propertyType)
                        : Expression.Constant(conditionVal, propertyType);
                }
            }


            switch (condition.Opt)
            {
                case "eq":
                    // return Expression.Equal(property,Expression.Convert(Expression.Constant(conditionVal), propertyType));
                    return Expression.Equal(property, conditionValExp);
                case "gt":
                    // return Expression.GreaterThan(property, Expression.Convert(Expression.Constant(conditionVal), propertyType));
                    // 字符串无法直接比较
                    return underlyingType != typeof(string)
                        ? Expression.GreaterThan(property, conditionValExp)
                        : Expression.GreaterThan(Expression.Call(_compareMethod, property, conditionValExp), Expression.Constant(0));
                case "gte":
                    // return Expression.GreaterThanOrEqual(property,Expression.Convert(Expression.Constant(conditionVal), propertyType));
                    return underlyingType != typeof(string)
                        ? Expression.GreaterThanOrEqual(property, conditionValExp)
                        : Expression.GreaterThanOrEqual(Expression.Call(_compareMethod, property, conditionValExp), Expression.Constant(0));
                case "lt":
                    // return Expression.LessThan(property,Expression.Convert(Expression.Constant(conditionVal), propertyType));
                    return underlyingType != typeof(string)
                        ? Expression.LessThan(property, conditionValExp)
                        : Expression.LessThan(Expression.Call(_compareMethod, property, conditionValExp), Expression.Constant(0));
                case "lte":
                    // return Expression.LessThanOrEqual(property,Expression.Convert(Expression.Constant(conditionVal), propertyType));
                    return underlyingType != typeof(string)
                        ? Expression.LessThanOrEqual(property, conditionValExp)
                        : Expression.LessThanOrEqual(Expression.Call(_compareMethod, property, conditionValExp), Expression.Constant(0));
                case "neq":
                    // return Expression.NotEqual(property,Expression.Convert(Expression.Constant(conditionVal), propertyType));
                    return Expression.NotEqual(property, conditionValExp);
                case "beg":
                    // return Expression.Call(property, "StartsWith", null, Expression.Convert(Expression.Constant(conditionVal), propertyType));
                    return Expression.Call(property, "StartsWith", null, conditionValExp);
                case "end":
                    // return Expression.Call(property, "EndsWith", null, Expression.Convert(Expression.Constant(conditionVal), propertyType));
                    return Expression.Call(property, "EndsWith", null, conditionValExp);
                case "like":
                    // return Expression.Call(property, "Contains", null, Expression.Convert(Expression.Constant(conditionVal), propertyType));
                    return Expression.Call(property, "Contains", null, conditionValExp);
                case "in":
                    // return Expression.Call(_containsMethod, Expression.Constant(conditionVal), property);
                    return Expression.Call(_containsMethod, conditionValExp, Expression.Convert(property, typeof(object)));
                case "nu":
                    return isnullType ? Expression.Equal(property, conditionValExp) : Expression.Constant(false);
                case "nnu":
                    return isnullType ? Expression.NotEqual(property, conditionValExp) : Expression.Constant(true);
                default:
                    throw new ArgumentException("System:无此条件符号", condition.Opt);
            }
        }

        private Expression CreateWhereSubGroupExpression(KaTableWhereCondition condition, ParameterExpression table)
        {
            Expression? exp = null;

            foreach (var conditionChild in condition.Children!)
            {
                var childExp = CreateWhereSubExpression(conditionChild, table);
                if (exp == null)
                {
                    exp = childExp;
                    continue;
                }

                exp = conditionChild.Bool == "and"
                    ? Expression.AndAlso(exp, childExp)
                    : Expression.OrElse(exp, childExp);
            }

            return exp!;
        }

        // 取得属性表达式
        private static MemberExpression? CreateExpressionProperty(string propertyName, Expression par)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            try
            {
                return (MemberExpression)propertyName.Split('.').Aggregate(par, Expression.PropertyOrField);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region 工具

        // 取得对象的属性值
        private static object GetPropertyValue(object obj, string propName)
        {
            var props = propName.Split('.').Select(ToCamel).ToArray();
            var len = props.Length;
            var type = obj.GetType();

            for (var i = 0; i < len; i++)
            {
                var p = type.GetProperty(props[i]);
                if (p == null) throw new Exception($"System:无此属性({propName})");

                type = p.PropertyType;
                obj = p.GetValue(obj)!;

                if (i == len - 1) return obj;
            }

            throw new Exception($"System:无此属性({propName})");
        }

        /// <summary>
        /// 将前端JSON转换成对象
        /// </summary>
        /// <param name="data">前端JSON</param>
        /// <param name="surfaceEntity">out 一层entity</param>
        /// <param name="isJustSurface">是否只转换一层</param>
        /// <returns>Dto</returns>
        private TDto? CreateObject(Dictionary<string, object> data, out TEntity? surfaceEntity,
            bool isJustSurface = false)
        {
            var nestedDictionary = new Dictionary<string, object>();

            foreach (var keyValuePair in data)
            {
                var keys = keyValuePair.Key.Split('.');
                var currentLevel = nestedDictionary;

                for (var i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];

                    if (i == keys.Length - 1)
                    {
                        currentLevel[key] = keyValuePair.Value;
                    }
                    else
                    {
                        if (!currentLevel.ContainsKey(key))
                        {
                            currentLevel[key] = new Dictionary<string, object>();
                        }

                        currentLevel = (Dictionary<string, object>)currentLevel[key];
                    }
                }
            }

            var str = JsonSerializer.Serialize(nestedDictionary);
            var result = isJustSurface ? null : JsonSerializer.Deserialize<TDto>(str, _jsonDesSetting)!;
            var entity = JsonSerializer.Deserialize<object>(str, _jsonSettingDepth1);
            str = JsonSerializer.Serialize(entity);
            surfaceEntity = JsonSerializer.Deserialize<TEntity>(str, _jsonDesSetting);

            return result;
        }

        private TTo Mapper<TFrom, TTo>(TFrom fromObj)
        {
            var fromType = typeof(TFrom);
            var toType = typeof(TTo);

            PropertyInfo[] fromProps;
            PropertyInfo[] toProps;

            if (fromType == typeof(TEntity))
            {
                fromProps = _entityProps.Values.ToArray();
            }
            else if (fromType == typeof(TDto))
            {
                fromProps = _dtoProps.Values.ToArray();
            }
            else
            {
                fromProps = fromType.GetProperties();
            }

            if (toType == typeof(TEntity))
            {
                toProps = _entityProps.Values.ToArray();
            }
            else if (toType == typeof(TDto))
            {
                toProps = _dtoProps.Values.ToArray();
            }
            else
            {
                toProps = toType.GetProperties();
            }

            var toObj = Activator.CreateInstance(toType);

            foreach (var fromProp in fromProps)
            {
                var toProp = toProps.FirstOrDefault(p => p.Name == fromProp.Name);
                if (toProp != null)
                {
                    toProp.SetValue(toObj, fromProp.GetValue(fromObj));
                }
            }

            return (TTo)toObj!;
        }

        // 判断属性是否是可空的，string?，int?
        private static bool IsNullable(PropertyInfo prop)
        {
            var propType = prop.PropertyType;

            if (!propType.IsClass) return Nullable.GetUnderlyingType(propType) != null;

            var nullabilityContext = new NullabilityInfoContext();
            var nullabilityInfo = nullabilityContext.Create(prop);
            return nullabilityInfo.WriteState is NullabilityState.Nullable;
        }

        // 首字母大写
        private static string ToCamel(string str)
        {
            // return $"{char.ToUpper(str[0])}{str.Substring(1)}";
            Span<char> span = str.ToCharArray().AsSpan();
            span[0] = char.ToUpper(span[0]);

            return new string(span);
        }

        // 按MaxDepth反序列化
        private T GetFlatObject<T>(string json, JsonSerializerOptions? options = null)
        {
            options ??= _jsonSettingDepth1;
            var entity =
                JsonSerializer.Deserialize<object>(json, options);
            var str = JsonSerializer.Serialize(entity);
            return JsonSerializer.Deserialize<T>(str, _jsonDesSetting)!;
        }

        // 将前端的value值转换成实际类型值
        private static object? GetJsonElementValue(JsonElement element, Type type)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => type switch
                {
                    null => null,
                    _ when type == typeof(DateTime) || type == typeof(DateTime?) => element.GetDateTime(),
                    _ when type == typeof(string) => element.GetString(),
                    _ => element.GetString()
                },
                JsonValueKind.Number => type switch
                {
                    null => null,
                    _ when type == typeof(int) || type == typeof(int?) => element.GetInt32(),
                    _ when type == typeof(uint) || type == typeof(uint?) => element.GetUInt32(),
                    _ when type == typeof(long) || type == typeof(long?) => element.GetInt64(),
                    _ when type == typeof(ulong) || type == typeof(ulong?) => element.GetUInt64(),
                    _ when type == typeof(short) || type == typeof(short?) => element.GetInt16(),
                    _ when type == typeof(ushort) || type == typeof(ushort?) => element.GetUInt16(),
                    _ when type == typeof(byte) || type == typeof(byte?) => element.GetByte(),
                    _ when type == typeof(float) || type == typeof(float?) => element.GetSingle(),
                    _ when type == typeof(double) || type == typeof(double?) => element.GetDouble(),
                    _ when type == typeof(decimal) || type == typeof(decimal?) => element.GetDecimal(),
                    _ => element.GetInt32()
                },
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => element.EnumerateArray().Select(l => GetJsonElementValue(l, type)).ToArray(),
                _ => null
            };
        }

        #endregion
    }

    #region class

    public class KaTableSearchParameter
    {
        public int PageSize { get; init; }
        public int PageNum { get; init; }
        public KaTableSortCondition[] SortConditions { get; init; } = null!;

        public KaTableWhereCondition[] WhereConditions { get; init; } = null!;
        // public KaTableSearchSummary[] SummaryConditions { get; set; } = null!;
    }

    public class KaTableExportParameter : KaTableSearchParameter
    {
        public KaTableExportCondition[] Cols { get; init; } = null!;
        public string FileName { get; init; } = null!;
    }

    public class KaTableImportCol
    {
        public string Key { get; set; } = null!;
        public string Title { get; init; } = null!;
    }

    public class KaTableResponse
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 異常信息
        /// </summary>
        public string Message { get; set; } = null!;
    }

    public class KaTableResponseRecord<T> : KaTableResponse
    {
        public T? Record { get; set; }
    }

    public class KaTableResponseRecords<T> : KaTableResponse
    {
        public List<T> Records { get; set; } = null!;
    }

    public class KaTableSearchResponse<T> : KaTableResponseRecords<T>
    {
        public int Total { get; set; }
        public object? Summary { get; set; }
    }


    public class KaTableSortCondition
    {
        public string Key { get; init; } = null!;
        /// <summary>
        /// ascend、descend
        /// </summary>
        public string Order { get; init; } = null!;
    }

    public class KaTableCustomSort<TEntity>
    {
        public Expression<Func<TEntity,object>> SortExpression { get; init; } = null!;
        public string Key { get; set; } = null!;
        /// <summary>
        /// ascend、descend
        /// </summary>
        public string Order { get; set; } = null!;
    }

    public class KaTableWhereCondition
    {
        public string Key { get; set; } = null!;
        public string Opt { get; set; } = null!;
        public JsonElement Val { get; set; }
        public string Bool { get; set; } = null!;
        public KaTableWhereCondition[]? Children { get; set; }
    }

    public class KaTableExportCondition
    {
        public string Key { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? DateFormat { get; set; }

        /** $row$, $col$ */
        public string? Formula { get; set; }
    }

    public class KaTableOptionItem
    {
        public string Label { get; set; } = null!;
        public object Value { get; set; } = null!;
        public bool Disabled { get; set; }
        public object Ext { get; set; } = null!;
    }

    public class KaTableSearchSummary
    {
        public string Key { get; set; } = null!;
        public string Summary { get; set; } = null!;
    }

    public record Lang
    {
        public required string TooManyRows { get; init; }
        public required string NoRecords { get; init; }
        public required string RecordChanged { get; init; }
        public required string ExcelTitle { get; init; }
    }

    #endregion
}