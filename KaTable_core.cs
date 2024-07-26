using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml;

namespace Sagacity.Utility
{
    public class KaTable<TEntity, TDto> where TEntity : class where TDto : class, new()
    {
        public static readonly Lang LangChinese = new Lang
        {
            TooManyRows = "有多笔数据",
            NoRecords = "无此记录或已变更",
            RecordChanged = "记录已变更",
            ExcelTitle="Excel第{0}列标题不是({1})",
        };
        public static readonly Lang LangEnglish = new Lang
        {
            TooManyRows = "There are multiple records",
            NoRecords = "No such record or changed",
            RecordChanged = "Record changed",
            ExcelTitle="Excel column {0} title is not ({1})",
        };
        
        // Contains的表达式
        private readonly MethodInfo _containsMethod =
            ((MethodCallExpression)((Expression<Func<IEnumerable<object>, bool>>)((q) =>
                q.Contains(null))).Body).Method;

        // 类属性
        private readonly Dictionary<string, PropertyInfo> _entityProps = new Dictionary<string, PropertyInfo>();
        private readonly Dictionary<string, PropertyInfo> _dtoProps = new Dictionary<string, PropertyInfo>();
        private readonly IList<PropertyInfo> _keyProps = new List<PropertyInfo>();

        // Linq的Include集合
        private readonly IList<Expression<Func<TEntity, object>>> _includeExps =
            new List<Expression<Func<TEntity, object>>>();

        // JSON反序列化设置
        private readonly JsonSerializerSettings _jsonDesSetting = new JsonSerializerSettings
        {
            // PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

        // JSON序列化设置
        private readonly JsonSerializerSettings _jsonSetting = new JsonSerializerSettings
        {
            // 排除null
            // DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NullValueHandling = NullValueHandling.Ignore,
            // 不循环引用
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            // ReferenceHandler = ReferenceHandler.IgnoreCycles,
            // 首字母小写
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            // PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // JSON反序列化第一层
        private readonly JsonSerializerSettings _jsonSettingDepth1 = new JsonSerializerSettings
        {
            MaxDepth = 1,
            // PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Error = (s, e) => e.ErrorContext.Handled = true
        };

        // JSON反序列化2层
        private readonly JsonSerializerSettings _jsonSettingDepth2 = new JsonSerializerSettings
        {
            MaxDepth = 2,
            // ReferenceHandler = ReferenceHandler.IgnoreCycles,
            // PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            Error = (s, e) => e.ErrorContext.Handled = true
        };

        private DbContext Db { get; }
        
        private Lang LangLocale { get; }

        /// <summary>
        /// 自定义Where
        /// </summary>
        public Expression<Func<TEntity, bool>>? CustomerWhere { get; set; }

        /// <summary>
        /// 自定义Select
        /// </summary>
        public Expression<Func<TEntity, TDto>>? CustomerSelect { get; set; }

        /// <summary>
        /// 自定义聚合,Total为总笔数,加入会加快，否则会产生两笔sum
        /// </summary>
        public Expression<Func<IGrouping<int, TEntity>, object>>? CustomerSummary { get; set; }

        /// <summary>
        /// 读取数据后
        /// </summary>
        public Action<IList<TDto>>? AfterLoadData { get; set; }

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
        /// 更新数据之前，newRecord, oldRecord
        /// </summary>
        public Action<TEntity, TDto>? BeforeUpdate { get; set; }

        /// <summary>
        /// 删除数据之前
        /// </summary>
        public Action<TEntity, TDto>? BeforeDelete { get; set; }

        /// <summary>
        /// 删除数据之后
        /// </summary>
        public Action<TEntity, TDto>? AfterDelete { get; set; }

        /// <summary>
        /// 导入之前
        /// </summary>
        public Action<TDto[]>? BeforeImport { get; set; }

        /// <summary>
        /// 导入之后
        /// </summary>
        public Action<TDto[]>? AfterImport { get; set; }

        /// <summary>
        /// 更新数据之后，newRecord, oldRecord
        /// </summary>
        public Action<TEntity, TDto>? AfterUpdate { get; set; }

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
                switch (act.ToString().ToUpper())
                {
                    case "SEARCH":
                        result = SearchAction(context);
                        break;
                    case "INSERT":
                        result = InsertAction(context);
                        break;
                    case "UPDATE":
                        result = UpdateAction(context);
                        break;
                    case "REMOVE":
                        result = DeleteAction(context);
                        break;
                    case "EXPORT":
                        result = ExportAction(context);
                        break;
                    case "IMPORT_FILE":
                        result = ImportFileAction(context);
                        break;
                    case "IMPORT":
                        result = ImportAction(context);
                        break;
                    case "DOWNLOAD_TEMPLATE":
                        result = DownloadTemplateAction(context);
                        break;
                    default:
                        throw new Exception("System:未提供此操作");
                }
            }
            catch (ValidationException ex)
            {
                result = Results.Json(new KaTableResponse
                {
                    IsSuccess = false,
                    Message = ex.ValidationResult.ErrorMessage ?? ex.Message
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

        public KaTable<TEntity, TDto> Include(Expression<Func<TEntity, object>> exp)
        {
            _includeExps.Add(exp);
            return this;
        }

        #region action

        private IResult SearchAction(HttpContext context)
        {
            var pars = context.Request.Form["searchPar"].ToString();
            if (string.IsNullOrEmpty(pars))
            {
                throw new Exception("System:没有参数searchPar");
            }

            // var searchPar = JsonConvert.DeserializeObject<KaTableSearchParameter>(pars,_jsonDesSetting);
            var searchPar = JsonConvert.DeserializeObject<KaTableSearchParameter>(pars, _jsonDesSetting);
            var result = SelectRecords(searchPar!);

            AfterLoadData?.Invoke(result.Records);

            return Results.Json(result);

            // context.Response.ContentType = "application/json";
            // context.Response.WriteAsJsonAsync(JsonSerializer.Serialize(result,_jsonSetting));
        }

        private IResult ExportAction(HttpContext context)
        {
            var pars = context.Request.Form["exportPar"];
            if (string.IsNullOrEmpty(pars))
            {
                throw new Exception("System:exportPar");
            }

            var exportPar = JsonConvert.DeserializeObject<KaTableExportParameter>(pars!, _jsonDesSetting);

            BeforeExportLoadData?.Invoke(exportPar!);
            var result = SelectRecords(exportPar!);
            BeforeExport?.Invoke(result.Records);

            var recordLen = result.Records.Count;
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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

            AfterExport.Invoke(worksheet, exportPar.Cols, recordLen);

            for (var i = 0; i < exportPar.Cols.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(exportPar.Cols[i].DateFormat))
                {
                    worksheet.Column(i + 1).Style.Numberformat.Format = exportPar.Cols[i].DateFormat;
                }
            }

            worksheet.Cells.AutoFitColumns();
            worksheet.Cells.Style.WrapText = true;

            // context.Response.Clear();
            // context.Response.Buffer = true;
            // context.Response.ContentEncoding = Encoding.UTF8;
            var stream = excel.GetAsByteArray();
            // context.Response.AddHeader("Content-Length", stream.Length.ToString());
            // context.Response.AppendHeader("File-Name", exportPar.FileName);
            // context.Response.BinaryWrite(stream);
            // context.Response.Flush();

            return Results.File(stream);
        }

        private IResult InsertAction(HttpContext context)
        {
            var data = context.Request.Form["record"];
            if (string.IsNullOrEmpty(data))
            {
                throw new Exception("System:没有参数record");
            }

            var dto = JsonConvert.DeserializeObject<TDto>(data!, _jsonDesSetting);

            // var record = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            // var dto = CreateObject(record, out var entity);

            BeforeInsert?.Invoke(dto!);
            // BeforeInsert改变dto之后再产生entity
            var entity = JsonConvert.DeserializeObject<TEntity>(JsonConvert.SerializeObject(dto), _jsonSettingDepth1);

            using (var transaction = Db.Database.BeginTransaction())
            {
                try
                {
                    InsertRecord(entity!);
                    AfterInsert?.Invoke(dto!);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }

            return Results.Json(
                new KaTableResponseRecord<TEntity> { IsSuccess = true, Record = entity! });
            // context.Response.ContentType = "application/json";
            // context.Response.Write(JsonSerializer.Serialize(
            //     new KaTableResponseRecord<TEntity> { IsSuccess = true, Record = entity }, _jsonSetting));
        }

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

            var oldDto = JsonConvert.DeserializeObject<TDto>(oldRecord!, _jsonDesSetting);
            var oldFlatDto = JsonConvert.DeserializeObject<TDto>(oldRecord!, _jsonSettingDepth1);

            using (var transaction = Db.Database.BeginTransaction())
            {
                try
                {
                    // 从前端的oldRecord读取数据中的旧数据
                    var dbRecord = GetEntityByJson(oldRecord!);
                    // 检查数据库中的数据时候和前端的oldRecord第一层相同
                    CheckOldRecord(dbRecord, oldFlatDto!);

                    // 将前端的newRecord数据更新至dbRecord
                    SetRecordVal(dbRecord,
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(newRecord!, _jsonSettingDepth1)!);
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
            // context.Response.ContentType = "application/json";
            // context.Response.Write(JsonSerializer.Serialize(
            //     new KaTableResponseRecord<TEntity> { IsSuccess = true, Record = resultObj }, _jsonSetting));
        }

        private IResult DeleteAction(HttpContext context)
        {
            var record = context.Request.Form["record"];
            if (string.IsNullOrEmpty(record))
            {
                throw new Exception("System:没有参数record");
            }

            var oldDto = JsonConvert.DeserializeObject<TDto>(record!, _jsonDesSetting);
            var oldFlatDto = JsonConvert.DeserializeObject<TDto>(record!, _jsonSettingDepth1);

            using (var transaction = Db.Database.BeginTransaction())
            {
                try
                {
                    // 从前端的oldRecord读取数据中的旧数据
                    var dbRecord = GetEntityByJson(record);
                    // 检查数据库中的数据时候和前端的oldRecord第一层相同
                    CheckOldRecord(dbRecord, oldFlatDto);

                    BeforeDelete?.Invoke(dbRecord, oldDto);

                    Db.Set<TEntity>().Remove(dbRecord);
                    Db.SaveChanges();

                    AfterDelete?.Invoke(dbRecord, oldDto);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }

            return Results.Json(new KaTableResponse { IsSuccess = true });

            // context.Response.ContentType = "application/json";
            // context.Response.Write(JsonSerializer.Serialize(new KaTableResponse { IsSuccess = true }, _jsonSetting));
        }

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

            var cols = JsonConvert.DeserializeObject<KaTableImportCol[]>(colsStr!, _jsonDesSetting);
            var result = GetRecordsByFile(cols, file);

            return Results.Json(result);

            // context.Response.ContentType = "application/json";
            // context.Response.Write(JsonSerializer.Serialize(result, _jsonSetting));
        }

        private IResult ImportAction(HttpContext context)
        {
            var data = context.Request.Form["records"];
            if (string.IsNullOrEmpty(data))
            {
                throw new Exception("System:没有参数records");
            }

            var dto = JsonConvert.DeserializeObject<TDto[]>(data!, _jsonDesSetting);

            // var record = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            // var dto = CreateObject(record, out var entity);

            BeforeImport?.Invoke(dto);
            var entity = JsonConvert.DeserializeObject<TEntity[]>(JsonConvert.SerializeObject(dto), _jsonSettingDepth2);

            using (var transaction = Db.Database.BeginTransaction())
            {
                try
                {
                    ImportRecord(entity);
                    AfterImport?.Invoke(dto);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }

            return Results.Json(new KaTableResponse { IsSuccess = true });

            // context.Response.ContentType = "application/json";
            // context.Response.Write(JsonSerializer.Serialize(new KaTableResponse { IsSuccess = true }, _jsonSetting));
        }

        private IResult DownloadTemplateAction(HttpContext context)
        {
            var colsStr = context.Request.Form["cols"];
            if (string.IsNullOrEmpty(colsStr))
            {
                throw new Exception("System:没有参数cols");
            }

            var cols = JsonConvert.DeserializeObject<KaTableImportCol[]>(colsStr!, _jsonDesSetting);
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var excel = new ExcelPackage())
            {
                var worksheet = excel.Workbook.Worksheets.Add("template");
                for (var i = 0; i < cols.Length; i++)
                {
                    var col = cols[i];
                    if (!string.IsNullOrEmpty(col.Title))
                    {
                        worksheet.Cells[1, i + 1].Value = col.Title;
                    }
                }

                worksheet.Cells.AutoFitColumns();
                worksheet.Cells.Style.WrapText = true;

                // context.Response.Clear();
                // context.Response.Buffer = true;
                // context.Response.ContentEncoding = Encoding.UTF8;
                var stream = excel.GetAsByteArray();
                // context.Response.AddHeader("Content-Length", stream.Length.ToString());
                // context.Response.BinaryWrite(stream);
                // context.Response.Flush();

                return Results.File(stream);
            }
        }

        #endregion

        #region test

        public Expression<Func<TEntity, bool>> TestCreateWhereExpression(KaTableWhereCondition[] conditions)
        {
            return CreateWhereExpression(conditions);
        }

        public KaTableSearchResponse<TDto> TestSelectRecords(KaTableSearchParameter searchPar)
        {
            return SelectRecords(searchPar);
        }

        public TDto TestCreateObject(Dictionary<string, object> data, out TEntity surfaceEntity)
        {
            return CreateObject(data, out surfaceEntity);
        }

        public Expression<Func<TEntity, bool>> TestCreateUpdateWhereExpression(Dictionary<string, object> record)
        {
            return CreateUpdateWhereExpression(record);
        }

        #endregion

        #region 方法

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
            // foreach (var condition in sortConditions)
            // {
            //     queryExp = CreateSortSubExpression(condition, queryExp, i++);
            // }

            data = data.Provider.CreateQuery<TEntity>(queryExp);

            // page
            if (searchPar.PageSize > 0)
            {
                data = data.Skip((searchPar.PageNum - 1) * searchPar.PageSize).Take(searchPar.PageSize);
            }

            // select
            //IQueryable<TDto> records = null;
            if (CustomerSelect != null)
            {
                result.Records = data.Select(CustomerSelect).ToList();
            }
            else if (typeof(TDto) == typeof(TEntity))
            {
                result.Records = (IList<TDto>)data.ToList();
            }
            else if (typeof(TEntity).IsSubclassOf(typeof(TDto)))
            {
                result.Records = data.ToList().ConvertAll(l => l as TDto);
            }
            else
            {
                result.Records = data.ToList().ConvertAll(Mapper<TEntity, TDto>);
            }

            // Console.WriteLine(records.Expression);

            return result;
        }

        private void InsertRecord(TEntity entity)
        {
            Db.Set<TEntity>().Add(entity);
            Db.SaveChanges();
        }

        private void SetRecordVal(TEntity entity, Dictionary<string, object> newRecord)
        {
            foreach (var col in newRecord)
            {
                var key = ToCamel(col.Key);
                if (!_entityProps.TryGetValue(key, out var entityProp))
                {
                    throw new Exception($"System:无此属性{key}");
                }

                if (!_dtoProps.ContainsKey(key))
                {
                    throw new Exception($"System:无此属性{key}");
                }

                entityProp.SetValue(entity, col.Value);
            }
        }

        private TEntity GetEntityByJson(string json)
        {
            var dbRecords = Db.Set<TEntity>()
                .Where(
                    CreateUpdateWhereExpression(
                        JsonConvert.DeserializeObject<Dictionary<string, object?>>(json, _jsonSettingDepth1)!
                            .Where(l => l.Value != null).ToDictionary(l => l.Key, l => l.Value)
                    )
                ).ToArray();
            if (dbRecords.Length > 1)
            {
                throw new Exception(LangLocale.TooManyRows);
            }

            if (dbRecords.Length == 0)
            {
                throw new Exception(LangLocale.NoRecords);
            }

            return dbRecords[0];
        }

        /// <summary>
        /// 检查旧数据是否是数据库里面的，防止前端篡改
        /// </summary>
        /// <param name="dbEntity">数据库实体</param>
        /// <param name="oldDto">旧数据</param>
        private void CheckOldRecord(TEntity dbEntity, TDto oldDto)
        {
            TDto newDto;

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
                //newDto = Mapper<TEntity, TDto>(dbEntity);
                newDto = new[] { dbEntity }.AsQueryable().Select(CustomerSelect).First();
            }


            var keys = _entityProps.Keys.Intersect(_dtoProps.Keys);

            foreach (var key in keys)
            {
                var oldVal = _entityProps[key].GetValue(oldDto);
                var newVal = _entityProps[key].GetValue(newDto);
                if (!Equals(oldVal, newVal))
                {
                    //throw new Exception($"数据已变更{key},{oldVal},{newVal}");
                    throw new Exception(LangLocale.RecordChanged);
                }
            }
        }

        private KaTableSearchResponse<TEntity> GetRecordsByFile(KaTableImportCol[] cols, IFormFile file)
        {
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using ExcelPackage package = new ExcelPackage(file.OpenReadStream());
            var sheet = package.Workbook.Worksheets[0];

            // var dtoType = typeof(TDto);
            for (var i = 0; i < cols.Length; i++)
            {
                var col = cols[i];
                if (!string.IsNullOrEmpty(col.Title))
                {
                    if (!string.Equals(sheet.Cells[1, i + 1].Value?.ToString(), col.Title,
                            StringComparison.CurrentCultureIgnoreCase))
                    {
                        throw new Exception(string.Format(LangLocale.ExcelTitle,i+1,col.Title));
                    }
                }

                // if (dtoType.GetProperty(col.Key) == null)
                // {
                //     throw new Exception($"无此列");
                // }
            }

            IList<TEntity> result = new List<TEntity>();
            var rowIndex = 2;
            while (true)
            {
                if (sheet.Cells[rowIndex, 1].Value == null) break;

                var dic = new Dictionary<string, object>();
                for (var colIndex = 0; colIndex < cols.Length; colIndex++)
                {
                    var col = cols[colIndex];
                    dic.Add(col.Key, sheet.Cells[rowIndex, colIndex + 1].Value);
                }

                CreateObject(dic, out var obj);
                result.Add(obj);

                rowIndex++;
            }

            return new KaTableSearchResponse<TEntity> { IsSuccess = true, Records = result, Total = result.Count };
        }

        private void ImportRecord(TEntity[] entities)
        {
            Db.Set<TEntity>().AddRange(entities);
            Db.SaveChanges();
        }

        #endregion

        #region 表达式树

        private Expression CreateSortSubExpression(KaTableSortCondition condition, in Expression oldExp,
            int sortIndex = 0)
        {
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
                new[] { typeof(TEntity), ((PropertyInfo)member.Member).PropertyType }, oldExp, ordLamb);
        }

        private Expression<Func<TEntity, bool>> CreateUpdateWhereExpression(Dictionary<string, object?> record)
        {
            var table = Expression.Parameter(typeof(TEntity), "t");

            Expression exp = null;
            var index = 0;

            foreach (var col in record)
            {
                if (col.Key.Contains(".")) continue;
                var property = CreateExpressionProperty(col.Key, table);
                if (property == null) continue;
                var propertyTypeRaw = ((PropertyInfo)property.Member).PropertyType;
                var propExpression = Expression.Equal(property,
                    Expression.Convert(Expression.Constant(col.Value), propertyTypeRaw));

                if (++index == 1)
                {
                    exp = propExpression;
                    continue;
                }

                Debug.Assert(exp != null, nameof(exp) + " != null");
                exp = Expression.AndAlso(exp, propExpression);
            }

            return Expression.Lambda<Func<TEntity, bool>>(exp ?? Expression.Constant(true), table);
        }

        private Expression<Func<TEntity, bool>> CreateWhereExpression(KaTableWhereCondition[] conditions)
        {
            var table = Expression.Parameter(typeof(TEntity), "t");

            Expression exp = null;
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

            // var table = Expression.Parameter(typeof(TEntity), "t");
            var property = CreateExpressionProperty(condition.Key, table);
            if (property == null) return Expression.Constant(true);
            var propertyTypeRaw = ((PropertyInfo)property.Member).PropertyType;
            // Console.WriteLine($"{condition.Col},{condition.Bool},{condition.Val}");
            // var propertyType = IsNullable(propertyTypeRaw)
            //     ? typeof(Nullable<>).MakeGenericType(propertyTypeRaw)
            //     : propertyTypeRaw;
            var propertyType = propertyTypeRaw;
            switch (condition.Opt)
            {
                case "eq":
                    return Expression.Equal(property,
                        Expression.Convert(Expression.Constant(condition.Val), propertyType));
                case "gt":
                    return Expression.GreaterThan(property,
                        Expression.Convert(Expression.Constant(condition.Val), propertyType));
                case "gte":
                    return Expression.GreaterThanOrEqual(property,
                        Expression.Convert(Expression.Constant(condition.Val), propertyType));
                case "lt":
                    return Expression.LessThan(property,
                        Expression.Convert(Expression.Constant(condition.Val), propertyType));
                case "lte":
                    return Expression.LessThanOrEqual(property,
                        Expression.Convert(Expression.Constant(condition.Val), propertyType));
                case "neq":
                    return Expression.NotEqual(property,
                        Expression.Convert(Expression.Constant(condition.Val), propertyType));
                case "beg":
                    return Expression.Call(property, "StartsWith", null,
                        Expression.Convert(Expression.Constant(condition.Val), propertyType));
                case "end":
                    return Expression.Call(property, "EndsWith", null,
                        Expression.Convert(Expression.Constant(condition.Val), propertyType));
                case "like":
                    return Expression.Call(property, "Contains", null,
                        Expression.Convert(Expression.Constant(condition.Val), propertyType));
                case "in":
                    return Expression.Call(_containsMethod, Expression.Constant(condition.Val), property);
                case "nu":
                    if (IsNullable(propertyType))
                    {
                        return Expression.Equal(property, Expression.Constant(null));
                    }

                    return Expression.Constant(false);
                case "nnu":
                    if (IsNullable(propertyType))
                    {
                        return Expression.NotEqual(property, Expression.Constant(null));
                    }

                    return Expression.Constant(true);
                default:
                    throw new ArgumentException("System:无此条件符号", condition.Opt);
            }
        }

        private Expression CreateWhereSubGroupExpression(KaTableWhereCondition condition, ParameterExpression table)
        {
            Expression exp = null;

            foreach (var conditionChild in condition.Children)
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

            return exp;
        }

        private MemberExpression CreateExpressionProperty(string propertyName, Expression par)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException(nameof(propertyName));
            var nameArr = propertyName.Split('.');
            try
            {
                var result = Expression.Property(par, nameArr[0]);
                for (var i = 1; i < nameArr.Length; i++)
                {
                    result = Expression.Property(result, nameArr[i]);
                }

                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private PropertyInfo GetProperty(Type type, string propName)
        {
            var props = propName.Split('.').Select(ToCamel).ToArray();
            var len = props.Length;

            for (var i = 0; i < len; i++)
            {
                var p = type.GetProperty(props[i]);
                if (p == null) return null;

                if (i == len - 1) return p;

                type = p.PropertyType;
            }

            return null;
        }

        private object GetPropertyValue(object obj, string propName)
        {
            var props = propName.Split('.').Select(ToCamel).ToArray();
            var len = props.Length;
            var type = obj.GetType();

            for (var i = 0; i < len; i++)
            {
                var p = type.GetProperty(props[i]);
                if (p == null) throw new Exception($"System:无此属性({propName})");

                type = p.PropertyType;
                obj = p.GetValue(obj);

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
        private TDto CreateObject(Dictionary<string, object> data, out TEntity surfaceEntity,
            bool isJustSurface = false)
        {
            var created = new Dictionary<string, object>();

            var type = typeof(TEntity);
            var instance = Activator.CreateInstance(type);
            var surface = Activator.CreateInstance(type);

            foreach (var col in data)
            {
                var obj = instance;
                var keyArr = col.Key.Split('.').Select(ToCamel).ToArray();
                var prop = type.GetProperty(keyArr[0]);
                if (prop == null)
                {
                    throw new Exception($"System:无此属性{keyArr[0]}");
                }

                // var valType = col.Value.GetType();
                var valStr = $"\"{col.Value}\"";

                if (keyArr.Length == 1)
                {
                    prop.SetValue(surface, JsonConvert.DeserializeObject(valStr, prop.PropertyType, _jsonSetting));
                }
                else
                {
                    if (!isJustSurface)
                    {
                        for (var i = 0; i < keyArr.Length; i++)
                        {
                            prop = obj.GetType().GetProperty(keyArr[i]);
                            if (prop == null)
                            {
                                throw new Exception($"System:无此属性{keyArr[i]}");
                            }

                            if (i == keyArr.Length - 1)
                            {
                                break;
                            }

                            var createdKey = string.Join(".", keyArr.Take(i));
                            if (created.TryGetValue(createdKey, out var value))
                            {
                                obj = value;
                                continue;
                            }

                            var propObj = Activator.CreateInstance(prop.PropertyType);
                            created.Add(createdKey, propObj);
                            prop.SetValue(obj, propObj);
                            obj = propObj;
                        }
                    }
                }

                if (!isJustSurface)
                {
                    prop.SetValue(obj, JsonConvert.DeserializeObject(valStr, prop.PropertyType, _jsonSetting));
                }

                // prop.SetValue(obj, valType == prop.PropertyType ? col.Value : (col.Value == null ? null : Convert.ChangeType(col.Value,prop.PropertyType)));
            }

            surfaceEntity = (TEntity)surface;
            return (TDto)instance;
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

            return (TTo)toObj;
        }

        #endregion

        private bool IsNullable(Type type)
        {
            if (type.IsClass)
                return true;
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private string ToCamel(string str)
        {
            return $"{char.ToUpper(str[0])}{str.Substring(1)}";
        }
    }


    public class KaTableSearchParameter
    {
        public int PageSize { get; set; }
        public int PageNum { get; set; }
        public KaTableSortCondition[] SortConditions { get; set; }
        public KaTableWhereCondition[] WhereConditions { get; set; }
        public KaTableSearchSummary[] SummaryConditions { get; set; }
    }

    public class KaTableExportParameter : KaTableSearchParameter
    {
        public KaTableExportCondition[] Cols { get; set; }
        public string FileName { get; set; }
    }

    public class KaTableImportCol
    {
        public string Key { get; set; }
        public string Title { get; set; }
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
        public string Message { get; set; }
    }

    public class KaTableResponseRecord<T> : KaTableResponse
    {
        public T Record { get; set; }
    }

    public class KaTableResponseRecords<T> : KaTableResponse
    {
        public IList<T> Records { get; set; }
    }

    public class KaTableSearchResponse<T> : KaTableResponseRecords<T>
    {
        public int Total { get; set; }
        public object Summary { get; set; }
    }


    public class KaTableSortCondition
    {
        public string Key { get; set; }
        public string Order { get; set; }
    }

    public class KaTableWhereCondition
    {
        public string Key { get; set; }
        public string Opt { get; set; }
        public object Val { get; set; }
        public string Bool { get; set; }
        public KaTableWhereCondition[] Children { get; set; }
    }

    public class KaTableExportCondition
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public string DateFormat { get; set; }

        /** $row$, $col$ */
        public string Formula { get; set; }
    }

    public class KaTableOptionItem
    {
        public string Label { get; set; }
        public object Value { get; set; }
        public bool Disabled { get; set; }
        public object Ext { get; set; }
    }

    public class KaTableSearchSummary
    {
        public string Key { get; set; }
        public string Summary { get; set; }
    }

    public record Lang
    {
        public required string TooManyRows { get; init; }
        public required string NoRecords { get; init; }
        public required string RecordChanged { get; init; }
        public required string ExcelTitle { get; init; }
    }
    
}