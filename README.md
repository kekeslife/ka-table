Ant design vue 中的table组件扩展，助力CRUD boy

# 快速开始

## 编译

npm run package

- ka-table.iife.js - 用于浏览器引用
- ka-table.js - 用于es模式引用



## 依赖

需要额外引入下列包

- dayjs
- axios
- vue
- ant-design-vue



## 引用

主要讲传统页面的引用方式

### 浏览器esm方式引用

```html
<script type="importmap">
{
    "imports":{
        "vue":"/scripts/vue3.4.21/vue.esm-browser.prod.js",
        "ant-design-vue":"/scripts/antd4.1.2/antd.esm.min.js",
        "dayjs":"/scripts/dayjs1.11.10/esm/dayjs-antd-vue.js",
        "axios":"/scripts/axios1.6.8/axios.esm.min.js",
    }
}
</script>
```

```html
<script type="module">
	import dayjs from 'dayjs';
	import axios from 'axios';
    import { createApp, reactive, ref, } from 'vue';
    import * as antd from 'ant-design-vue';
    import zhCN from '/Scripts/antd4.1.2/zh-CN.js';
    import katable from '/scripts/katable/ka-table.js';
</script>
```

中文处理方式

需要重新打包dayjs, antd zh_CN

```html
<script type="module">
    import zhCN from '/Scripts/antd4.1.2/zh-CN.js';
    
    dayjs.locale('zh-cn');
</script>
```



## esm重新打包

浏览器esm方式，dayjs, antd vue无法使用中文，所以需要将中文重新打包

新建vite项目

```javascript
// vite.config.ts
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path';

export default defineConfig({
  plugins: [vue()],
  build: {
		lib: {
			entry: resolve(__dirname, 'src/main.ts'),
			
			 // name: 'zh-CN',
			 // fileName: 'zh-CN',
			// rollupOptions: {
			// 	external: ['dayjs'],
			// },
			
			name:'dayjs-antd-vue',
			fileName: 'dayjs-antd-vue',
			
			formats: ['es'],
		},
	},
})
```

### antd vue zh_CN

```javascript
// main.ts
import zhCN from 'ant-design-vue/es/locale/zh_CN';
export default zhCN;
```

### dayjs

```javascript
// main.ts
import dayjs from 'dayjs';
import localeData from 'dayjs/plugin/localeData';
import weekday from 'dayjs/plugin/weekday';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import weekOfYear from 'dayjs/plugin/weekOfYear';
import weekYear from 'dayjs/plugin/weekYear';
import advancedFormat from 'dayjs/plugin/advancedFormat';
import quarterOfYear from 'dayjs/plugin/quarterOfYear';
import timezone from 'dayjs/plugin/timezone';
import utc from 'dayjs/plugin/utc';
import  'dayjs/locale/zh-cn';

dayjs.extend(localeData);
dayjs.extend(weekday);
dayjs.extend(customParseFormat);
dayjs.extend(weekOfYear);
dayjs.extend(weekYear);
dayjs.extend(advancedFormat);
dayjs.extend(quarterOfYear);
dayjs.extend(timezone);
dayjs.extend(utc);

export default dayjs;
```

## 示例

### HTML

```html
<div id="app">
    <ka-table
        ref="$table"
        :locale="zhCN" 		// antd中文
        theme="#123456"		// 主题色
        table-title="需求單" // 表格标题
        size="small" 		// 紧凑模式
        :columns="columns"	// 字段配置
        :page-size="20"		// 表格行数
        :toolbar="toolbar"	// 开关控制栏
        :is-debug="true"	// debug模式，输出一些信息
        url="demandv2.ashx"	// 后台api地址
        :on-after-edit="onAfterEdit"		// 点击编辑按钮，初始化编辑项之后
        :on-before-add="onBeforeAdd"		// 点击添加按钮，初始化编辑项之前
        :on-post-load-data="onPostLoadData"	// 读取数据之后
        :on-pre-add="onPreAdd"				// post insert数据之前
        :on-post-add="onPostAdd"			// post insert数据之后
        :on-pre-add-or-edit ="onPreAddOrEdit"	// post insert或update数据之前
        :init-sorter-conditions="[{ key: 'createDateTime', order: 'descend', index: 1 },]"	// 初始化排序
    >
        <!-- 自定义控制栏扩展 -->
        <template #toolbar="{ dataSource }">
            <a-space-compact size="small">
                <a-tooltip><a-button @click="searchType('1')">修改程序</a-button></a-tooltip>
                <a-tooltip><a-button @click="searchType('2')">修改资料</a-button></a-tooltip>
                <a-tooltip><a-button @click="searchType('3')">账号管理</a-button></a-tooltip>
            </a-space-compact>
        </template>
        <!-- 自定义单元格显示 -->
        <template #[slots.body_item]="{ column, text, record }">
            <a v-if="column.key === 'reqNo'" target="_blank" :href="record.filePath || null"><span :class="{grey:!record.filePath}">{{ record.reqNo }}</span></a>
            <span v-else-if="column.key === 'reqEmpNo'">{{record.reqEmpNo}}-{{record.reqEmp?.empName}}</span>
            <span v-else-if="column.key === 'reqDeptNo'">{{record.reqDeptNo}}-{{record.reqDept?.deptName}}</span>
            <span v-else-if="column.key === 'ypcEmpNo'">{{record.ypcEmpNo}}-{{record.ypcEmp?.empName}}</span>
        </template>
        <!-- 自定义汇总栏 -->
        <template #[slots.summary_bar]="{ summary }">
            <a-table-summary-row>
                <a-table-summary-cell>Total</a-table-summary-cell>
                <a-table-summary-cell>
                  <a-typography-text type="danger">{{ summary.maxReqNo }}</a-typography-text>
                </a-table-summary-cell>
            </a-table-summary-row>
        </template>
    </ka-table>

</div>
```

### JavaScript

```javascript
const app = createApp({
	// 注册组件
	components: {
        'ka-table': katable
    },
    setup() {
    	// 使用expose
    	const $table = ref();
		
		// 修复vue slot在esm方式中无法使用驼峰名
        const slots = {
            body_item: 'bodyItem',
            summary_bar:'summaryBar',
        };
        
        // 固定列表项通过后台获取
        const modeOptions = ref([]), deptOptions = ref([]), teamOptions = ref([]), allTeamOptions = ref([]);
        
        // 字段配置
        const columns = reactive({
        	// 【编号】字段，对应后台实体的【ReqNo】属性
        	reqNo: {
                title: '编号',
                dbInfo: {
                    isPk: true, // 主键
                },
                listInfo: {
                    index: 0,
                    width: '120px',
                    align: 'center',
                },
                sortInfo: {
                    index: null, // 开启排序功能
                },
                editorInfo: {
                    index: 2,
                    width: '200px',
                    display: 'readonly', // 只读，因为【编号】是自动编的
                    position: 'cling',	 // 显示位置和前一个字段紧贴在一起
                },
                //importInfo: {
                //    index: 1,
                //    title: '编号',
                //},
            },
            tMode: {
                title: '模组(保存后产生编号)',
                editorInfo: {
                    index: 1,
                    width: '100px',
                    position: 'inline',
                    rules: [{ required: true, message: '请选择模组' }],
                    componentType: 'select',
                    isPost: false, // 非实体属性，只是前端的临时字段
                    options: modeOptions.value,
                },
            },
            reqEmpNo: {
                title: '申请人',
                listInfo: {
                    index: 3,
                    width: '120px',
                },
                editorInfo: {
                    index: 3,
                    width: '200px',
                    rules: [{ required: true, message: '请输入申请人工号' }],
                    componentType: 'select',
                    options: async (key) => {
                        const res = await axios.post('demandv2.ashx', Qs.stringify({ actNo: 'getEmp', key }));
                        if (res.data.isSuccess) {
                            return res.data.records;
                        }
                        return [];
                    }, // 根据输入的内容动态显示选项列表
                    onAfterChange: async (val, opt) => {
                    	// 【申请人】改变后将选项列表中的ext赋值给【部门代码】
                        await $table.value.setEditorVal('reqDeptNo', opt.ext, true, true);
                    }
                },
                //importInfo: {
                //    index: 1,
                //    title: '申请人',
                //},
            },
            reqEmp: {
                empName: {
                    title: '申请人姓名',
                }
            },
            reqDeptNo: {
                title: '申请部门',
                listInfo: {
                    index: 2,
                    width: '120px',
                },
                editorInfo: {
                    index: 5,
                    width: '300px',
                    position: 'inline',
                    rules: [{ required: true, message: '请输入申请部门' }],
                    componentType: 'select',
                    options: deptOptions,
                    attrs: { optionFilterProp: 'label' }, // 按照label过滤选项
                },
            },
            reqDept: {
                deptName: {
                    title: '申请部门',
                }
            },
            reqDate: {
                title: '接单日期',
                dbInfo: {
                    dataType: 'date',
                    dateFormat: 'YYYY-MM-DD'
                },
                editorInfo: {
                    index: 7,
                    width: '120px',
                    rules: [{ required: true, message: '请输入接单日期' }],
                    componentType: 'date',
                },
            },
            reqHopeDate: {
                title: '希望完成日期',
                dbInfo: {
                    dataType: 'date',
                    dateFormat: 'YYYY-MM-DD'
                },
                listInfo: {
                    index: 4,
                    width: '140px',
                    align:'center',
                },
                editorInfo: {
                    index: 8,
                    width: '120px',
                    rules: [{ required: true, message: '请输入希望完成日期' }],
                    componentType: 'date',
                    position: 'inline',
                },
                sortInfo: {
                    index: null,
                },
            },
            type: {
                title: '需求类别',
                editorInfo: {
                    index: 0,
                    width: '200px',
                    rules: [{ required: true, message: '请选择需求类别' }],
                    componentType: 'select',
                    options: typeOptions,
                    onAfterChange: async (val, opt) => {
                        if (val === '2') {
                            await $table.value.setEditorVal('tMode', 'DATA', false, false);
                        } else if (val === '3') {
                            await $table.value.setEditorVal('tMode', 'ACCT', false, false);
                        }
                    }
                },
            },
            content: {
                title: '需求大纲',
                listInfo: {
                    index: 1,
                    width: null,
                },
                editorInfo: {
                    index: 9,
                    width: '600px',
                    rules: [{ required: true, message: '请输入需求大纲' }],
                    componentType: 'textarea',
                },

            },
            ypcEmpNo: {
                title: '负责人',
                listInfo: {
                    index: 5,
                    width: '120px',
                },
                editorInfo: {
                    index: 10,
                    width: '200px',
                    rules: [{ required: true, message: '请选择负责人' }],
                    componentType: 'select',
                    options: teamOptions.value,
                    attrs: { optionFilterProp:'label'},
                },
                filterInfo: {
                    isFilter: true,
                    options: allTeamOptions, // 筛选时允许所有选项，编辑时不能选择停用的选项
                }
            },
            ypcEmp: {
                empName: {
                    title: '负责人姓名',
                }
            },
            tcDate: {
                title: '预计完成日期',
                dbInfo: {
                    dataType: 'date',
                    dateFormat: 'YYYY-MM-DD'
                },
                editorInfo: {
                    index: 12,
                    width: '120px',
                    rules: [{ required: true, message: '请输入预计完成日期' }],
                    componentType: 'date',
                    position: 'inline',
                },
            },
            resMessage: {
                title: '资讯说明',
                editorInfo: {
                    index: 13,
                    width: '600px',
                    componentType: 'textarea',
                },
            },
            status: {
                title: '状态',
                listInfo: {
                    index: 7,
                    width: '65px',
                    align: 'center',
                    options: statusOptions,
                },
                editorInfo: {
                    index: 14,
                    width: '80px',
                    componentType: 'select',
                    onAfterChange:async (val, opt) => {
                        // 取得编辑项
                        const editor = $table.value.getEditorObj();
                        // 如果状态2、3，【完成日期】则允许输入
                        if (['2','3'].includes(val)) {
                            editor.scDate.display = 'show';
                        } else {
                            editor.scDate.display = 'readonly';
                            // 清空【完成日期】
                            await $table.value.setEditorVal('scDate', null, false, false);
                        }
                    },
                    rules: [{ required: true, message: '请选择状态' }],
                },
            },
            scDate: {
                title: '完成日期',
                dbInfo: {
                    dataType:'date',
                    dateFormat:'YYYY-MM-DD'
                },
                listInfo: {
                    index: 6,
                    width: '110px',
                    align: 'center',
                },
                editorInfo: {
                    index: 15,
                    width: '120px',
                    componentType: 'date',
                    position: 'inline',
                    rules: [{ validator: onValidateScDate, trigger: 'change' }],
                },
                sortInfo: {
                    index: null,
                },
            },
            createDateTime: {
                title: '建立日期',
                sortInfo: {
                    index:null,
                }
            },
            filePath: {
                title: '附件路径',
            }
        });
        
        // 点击添加按钮，初始化编辑项之前
        const onBeforeAdd = () => {
            // 取得编辑项
            const editor = $table.value.getEditorObj();
            // 【完成日期】只读
            editor.scDate.display = 'readonly';
            // 【需求类别】可输入
            editor.type.display = 'show';
            // 【模组】可输入
            editor.tMode.display = 'show';
            // 需要返回
            return { isSuccess: true };
        }
        
        // 点击添加按钮，初始化编辑项之后
        const onAfterEdit = (dataSource) => {
            const editor = $table.value.getEditorObj();
            if (['2','3'].includes(dataSource.curRecord.status)) {
                editor.scDate.display = 'show';
            } else {
                editor.scDate.display = 'readonly';
            }
            editor.type.display = 'readonly';
            editor.tMode.display = 'readonly';
            return { isSuccess: true };
        }
        
        // 发送insert之前
        const onPreAdd = async (dataSource) => {
            // 后台取得【编号】，赋值给【编辑】项
            const res = await axios.post('demandv2.ashx', Qs.stringify({
                actNo: 'getReqNo',
                mode: dataSource.editorRecord.tMode,
                ym: dataSource.editorRecord.reqDate.format('YYMM')
            }));
            if (!res.data.isSuccess) {
                return { isSuccess: false, message: res.data.message };
            }
            await $table.value.setEditorVal('reqNo', res.data.record, false, false);
            return { isSuccess: true };
        }

        // 发送insert之后
        const onPostAdd = (editorRecord) => {
            // 提示后台自动产生的【编号】
            return { isSuccess: true,message:editorRecord.reqNo,alertType:'alert' };
        }
        
        // 读取数据之后
        const onPostLoadData = (dataSource) => {
            // 临时字段【模组】，给值【编号】前4位
            for (const data of dataSource.records) {
                data.tMode = data.reqNo.substr(0, 4);
            }
            return { isSuccess: true };
        }
        
        // 发送insert、update之前
        const onPreAddOrEdit = (dataSource) => {
            if (dataSource.editorRecord.reqHopeDate.isBefore(dataSource.editorRecord.reqDate)) {
                return { isSuccess: false, message: '希望完成日期 < 接单日期' };
            }
            if (dataSource.editorRecord.tcDate.isBefore(dataSource.editorRecord.reqDate)) {
                return { isSuccess: false, message: '预计完成日期 < 接单日期' };
            }
            if (dataSource.editorRecord.scDate.isBefore(dataSource.editorRecord.reqDate)) {
                return { isSuccess: false, message: '完成日期 < 接单日期' };
            }
            return { isSuccess: true };
        }
        
        // 【实际完成日期】输入校验
        function onValidateScDate(rule, value) {
            if (!value) {
                const status = $table.value.getEditorVal('status');
                if (status === '2' || status === '3') {
                    return Promise.reject('请输入实际完成日期');
                }
            }
            return Promise.resolve();
        }
        
        // 初始化选规项
        const initOptions = async () => {
            const res = await axios.post('demandv2.ashx', Qs.stringify({ actNo: 'getOptions' }));
            if (res.data.isSuccess) {
                modeOptions.value.push(...res.data.record.mode);
                deptOptions.value.push(...res.data.record.dept);
                teamOptions.value.push(...res.data.record.team);
                allTeamOptions.value.push(...res.data.record.team.map(l => ({ label: l.label, value: l.value })));
            };
        };
        
        // 自定义控制栏，按【需求类别】筛选数据
        const searchType = async(type) => {
            $table.value.setFilters([{ key: 'type', opt: 'eq', val: type, bool: 'and' },]);
            await $table.value.reloadData();
        }

        return {
            columns, $table, slots, test, initOptions, zhCN, onAfterEdit, onBeforeAdd, onPostLoadData, onPreAdd, onPostAdd, onPreAddOrEdit,
            onValidateScDate, searchType,
        }
    },
    beforeMount() {
        this.initOptions();
    },
});

app.use(antd).mount('#app');
```

### .Net45 + EF6

```c#
// controller api

public override void Process(HttpContext context)
{
    string act = context.Request.Form["actNo"];
    var jsonSettings = new JsonSerializerSettings
    {
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
        NullValueHandling=NullValueHandling.Ignore
    };
    object res;
    switch (act)
    {
        case "getEmp":
            res = GetEmp(context.Request.Form["key"]);
            context.Response.ContentType = "text/plain";
            context.Response.Write(JsonConvert.SerializeObject(res, jsonSettings));
            break;
        case "getOptions":
            res = GetOptions();
            context.Response.ContentType = "text/plain";
            context.Response.Write(JsonConvert.SerializeObject(res, jsonSettings));
            break;
        case "getReqNo":
            res = GetReqNo(context.Request.Form["mode"], context.Request.Form["ym"]);
            context.Response.ContentType = "text/plain";
            context.Response.Write(JsonConvert.SerializeObject(res, jsonSettings));
            break;
        default:
            using (var db = new OracleEF())
            {
                var table = new KaTable<ItDemand, ItDemandDto>(db);
                // ef包含关联表
                table.Include(t => t.ReqEmp).Include(t=>t.ReqDept).Include(t => t.YpcEmp);
                // 自定义聚合
                table.CustomerSummary = t => new
                {
                    Total = t.Count(),
                    MaxReqNo = t.Max(d=>d.ReqNo)
                };
                // 读取数据之后查询时候有附件
                table.AfterLoadData = (records) =>
                {
                    for(var i = 0; i < records.Count; i++)
                    {
                        records[i].FilePath = ypcBase.FileInfo.SearchFile(records[0].ReqNo,"~/itweb/req/req");
                    }
                };
                // insert之前赋值【修改人】，【修改时间】
                table.BeforeInsert = (e) =>
                {
                    e.ModifyEmpNo = e.CreateEmpNo = new UserSession().User;
                    e.ModifyDateTime = e.CreateDateTime = DateTime.Now;
                };
                // update之前赋值【修改人】，【修改时间】
                table.BeforeUpdate = (e,old) =>
                {
                    e.ModifyEmpNo = new UserSession().User;
                    e.ModifyDateTime = DateTime.Now;
                };
                // 导出数据之后，将【状态】列转换成中文
                table.AfterExport = (worksheet, conditions, tot) =>
                {
                    // 【状态】列在excel第几列
                    var colIndex = Array.FindIndex(conditions, c => c.Key == "status");
                    if (colIndex == -1) return;
                    // 列1开始，需要+1
                    colIndex++;

                    var status = new Dictionary<string, string>
                    {
                        { "0", "作废" },
                        { "1", "登记" },
                        { "2", "完成" },
                        { "3", "结案" },
                    };

                    for (var i = 0; i < tot; i++)
                    {
                        if (status.TryGetValue(worksheet.Cells[i+2, colIndex].Value?.ToString() ?? "", out var val))
                        {
                            worksheet.Cells[i+2, colIndex].Value = val;
                        }
                    }
                };
                table.Action(context);
            }
            break;
    }
}

// 查询员工
private KaTableResponseRecords<KaTableOptionItem> GetEmp(string key)
{
    var result = new KaTableResponseRecords<KaTableOptionItem> { IsSuccess = true };

    try
    {
        using (var db = new OracleEF())
        {
            var emps = db.Emp.AsNoTracking().Where(d => d.EmpNo.StartsWith(key)).AsEnumerable();
            // Ext存放部门数据
            result.Records = emps.Select(l => new KaTableOptionItem { Label = $"{l.EmpNo} - {l.EmpName}", Value = l.EmpNo, Ext = l.DeptNo == "S001" ? "S001" : l.DeptNo.Substring(0,3) }).ToList();
        }
    }
    catch(Exception ex)
    {
        result.IsSuccess = false;
        result.Message = ex.Message;
    }

    return result;
}

// 初始化选项
private KaTableResponseRecord<object> GetOptions()
{
    var result = new KaTableResponseRecord<object> { IsSuccess = true };

    try
    {
        using (var db = new OracleEF())
        {
            var depts = db.Dept.AsNoTracking().Where(d => d.DeptNo.Length == 3 || d.DeptNo == "S001").Where(d => d.DeptNo != "YPC").OrderBy(d=>d.IsDel).ThenBy(d=>d.DeptNo).AsEnumerable();
            var deptOptions = depts.Select(l => new KaTableOptionItem { Label = $"{(l.IsDel == "Y"?"(停)":"")}{l.DeptNo} - {l.DeptName}", Value = l.DeptNo }).ToList();

            var modes = db.Database.SqlQuery<string>("SELECT DISTINCT SUBSTR(PROG_CODE,1,4) FROM MENU_PROGRAM WHERE MGT_CODE = 'CIM000MNU' ORDER BY SUBSTR(PROG_CODE,1,4)").AsEnumerable();
            var modeOptions = modes.Select(l => new KaTableOptionItem { Label = l, Value = l }).ToList();

            var team = db.Emp.AsNoTracking().Where(d => d.DeptNo.StartsWith("D11")).OrderByDescending(d=>d.Situation).ThenBy(d => d.DeptNo).AsEnumerable();
            // 非在职人员设置禁用Disabled
            var teamOptions = team.Select(l => new KaTableOptionItem { Label = $"{l.EmpNo} - {l.EmpName}", Value = l.EmpNo, Disabled= l.Situation != "P" }).ToList();
            result.Record = new
            {
                Dept = deptOptions,
                Mode = modeOptions,
                Team = teamOptions
            };
        }
    }
    catch (Exception ex)
    {
        result.IsSuccess = false;
        result.Message = ex.Message;
    }

    return result;
}

// Dto类，在Entity基础上扩展了文件路径
private class ItDemandDto: ItDemand
{
    public string FilePath { get; set; }
}
```

```c#
// Oracle EF 配置上下文
public class OracleEF : DbContext
{
    public OracleEF(string connStr = "SINOCNT-EF") : base(connStr)
    {
    	// 关闭EF6建立数据库，以数据库优先
        Database.SetInitializer<OracleEF>(null);
#if DEBUG
        Database.Log = Console.WriteLine;
#endif
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(string.Empty);
        modelBuilder.Entity<DocFlowRec>()
            .HasKey(b => new { b.Code, b.Item });

    }

    public DbSet<DocFlow> DocFlow { get; set; } = null;
    public DbSet<DocFlowRec> DocFlowRec { get; set; } = null;
    public DbSet<Emp> Emp { get; set; } = null;
    public DbSet<Dept> Dept { get; set; } = null;
    public DbSet<ItDemand> ItDemand { get; set; } = null;
}
```

```c#
// ItDemand实体类
[Table("EIP_IT_DEMAND")]
public class ItDemand:BaseTable
{
    [Key, Column("REQ_NO")]
    public string ReqNo { get; set; }

    [Column("REQ_EMP_NO")]
    public string ReqEmpNo { get; set; }

    [Column("REQ_DEPT_NO")]
    public string ReqDeptNo { get; set; }

    [Column("TYPE")]
    public string Type { get; set; }

    [Column("REQ_DATE")]
    public DateTime ReqDate { get; set; }

    [Column("REQ_HOPE_DATE")]
    public DateTime ReqHopeDate { get; set; }

    [Column("TC_DATE")]
    public DateTime? TcDate { get; set; }

    [Column("SC_DATE")]
    public DateTime? ScDate { get; set; }

    [Column("YPC_EMP_NO")]
    public string YpcEmpNo { get; set; } = null;

    [Column("STATUS")]
    public string Status { get; set; }

    [Column("CONTENT")]
    public string Content { get; set; } = null;

    [Column("RES_MESSAGE")]
    public string ResMessage { get; set; } = null;


    [ForeignKey("ReqEmpNo")]
    public Emp ReqEmp { get; set; }

    [ForeignKey("YpcEmpNo")]
    public Emp YpcEmp { get; set; }

    [ForeignKey("ReqDeptNo")]
    public Dept ReqDept { get; set; }
}
```



# API

## .Net

| 属性                 | 说明                                                         |
| -------------------- | ------------------------------------------------------------ |
| CustomerWhere        | 自定义EF Where                                               |
| CustomerSelect       | 自定义EF Select                                              |
| CustomerSummary      | 自定义聚合，可在前端summary中取得，Total为总行数，请务必加入 |
| AfterLoadData        | 读取数据后                                                   |
| BeforeExportLoadData | 导出，在查询数据之前                                         |
| BeforeExport         | 导出，在产生Excel之前                                        |
| AfterExport          | 导出，在产生Excel之后                                        |
| BeforeInsert         | 新增数据之前                                                 |
| AfterInsert          | 新增数据之后                                                 |
| BeforeUpdate         | 更新数据之前                                                 |
| AfterUpdate          | 更新数据之后                                                 |
| BeforeDelete         | 删除数据之前                                                 |
| AfterDelete          | 删除数据之后                                                 |
| BeforeImport         | 导入数据之前                                                 |
| AfterImport          | 导入数据之后                                                 |

## JavaScript

### 插槽

| 插槽       | 说明                 |
| ---------- | -------------------- |
| 其它       | Antd vue table的插槽 |
| summaryBar | 汇总区               |
| bodyItem   | 单元格自定义内容渲染 |
| toolbar    | 工具栏扩展           |

### Expose

| 属性                                                         | 说明                           |
| ------------------------------------------------------------ | ------------------------------ |
| reRenderEditor()                                             | 重新渲染编辑区                 |
| getEditorVal (key?: 字段)                                    | 取得编辑项数据，空字段取得所有 |
| setEditorVal (key: 字段, val: 值, trigChange?: 是否触发onChange, trigSearch?: 是否触发onSearch) | 设置编辑项数据                 |
| getEditorObj()                                               | 取得所有编辑项配置             |
| getAntCols()                                                 | 取得ant table columns配置      |
| reloadData()                                                 | async 刷新数据                 |
| reinit()                                                     | 重新初始化配置                 |
| setFilters(conditions:条件)                                  | 设置筛选条件                   |
| setSorters(conditions:条件)                                  | 设置排序条件                   |
| resetFields (name?: 字段)                                    | ant重置编辑项                  |
| clearValidate(name?: 字段)                                   | ant清空校验                    |
| validate(nameList?:字段, options:?设置)                      | ant校验编辑项                  |
| validateFields(nameList?:字段, options:?设置)                | ant校验编辑项                  |
| scrollToField(name?:字段, options:?设置)                     | ant滚动至编辑项                |

### Props

| 属性                                | 说明                                             |
| ----------------------------------- | ------------------------------------------------ |
| locale                              | ant的国际化                                      |
| size                                | ant尺寸                                          |
| columns                             | 字段                                             |
| tableTitle: string                  | 表格标题                                         |
| theme: string                       | 主题颜色码                                       |
| url: string                         | 后台API地址                                      |
| pageSize: number                    | 每页行数                                         |
| autoLoad: boolean                   | 读取数据后是否自动加载数据                       |
| isDebug: boolean                    | 调试模式                                         |
| refreshTitle: string                | 刷新按钮标题                                     |
| addTitle: string                    | 新增按钮标题                                     |
| editTitle: string                   | 修改按钮标题                                     |
| sortTitle: string                   | 排序按钮标题                                     |
| filterTitle: string                 | 筛选按钮标题                                     |
| removeTitle: string                 | 删除按钮标题                                     |
| importTitle: string                 | 导入按钮标题                                     |
| exportTitle: string                 | 导出按钮标题                                     |
| exportFilename: ()=>string          | 导出文件名                                       |
| initFilterConditions: condition[]   | 初始过滤条件                                     |
| initSorterConditions: condition[]   | 初始排序条件                                     |
| frozenFilterConditions: condition[] | 固定过滤条件                                     |
| toolbar:{}                          | 工具栏功能开关                                   |
| onBeforeRefresh                     | 点击刷新按钮，刷新数据之前                       |
| onPostRefresh                       | 刷新数据之后                                     |
| onPostLoadData                      | 读取数据之后                                     |
| onBeforeFilter                      | 点击筛选按钮，显示筛选框之前                     |
| onAfterFilter                       | 点击筛选按钮，显示筛选框之后                     |
| onPreFilter                         | 提交筛选之前                                     |
| onPostFilter                        | 提交筛选之后                                     |
| onBeforeSort                        | 点击排序按钮，显示排序框之前                     |
| onAfterSort                         | 点击排序按钮，显示排序框之后                     |
| onPreSort                           | 提交排序之前                                     |
| onPostSort                          | 提交排序之后                                     |
| onBeforeEdit                        | 点击修改按钮，数据填入之前，用于判断是否可以修改 |
| onAfterEdit                         | 点击修改按钮，数据填入之后，默认值可在这里设置   |
| onPreEdit                           | 提交Update之前                                   |
| onPostEdit                          | 提交Update之后                                   |
| onBeforeAdd                         | 点击新增按钮，数据填入之前                       |
| onAfterAdd                          | 点击新增按钮，数据填入之后                       |
| onPreAdd                            | 提交Insert之前                                   |
| onPostAdd                           | 提交Insert之后                                   |
| onPreAddOrEdit                      | 提交Insert或Update之前                           |
| onPostAddOrEdit                     | 提交Insert或Update之后                           |
| onPreRemove                         | 提交Remove之前                                   |
| onPostRemove                        | 提交Remove之后                                   |
| onBeforeExport                      | 点击导出按钮，显示确认之前                       |
| onAfterExport                       | 点击导出按钮，确认之后                           |
| onPreExport                         | 提交Export之前                                   |
| onPostExport                        | 提交Export之后                                   |
| onBeforeImportFile                  | 点击导入按钮，选择文件之前                       |
| onAfterImportFile                   | 点击导入按钮，选择文件之后                       |
| onPreImportFile                     | 提交解析Excel文件之前                            |
| onPostImportFile                    | 提交解析Excel文件之后                            |
| onPreImport                         | 提交导入数据之前                                 |
| onPostImport                        | 提交导入数据之后                                 |
| onPrePage                           | 点击分页按钮，提交分页之前                       |
| onPostPage                          | 点击分页按钮，提交分页之后                       |
| onAfterRowClick                     | 点击行之后                                       |
| onAfterRowDbClick                   | 双击行之后                                       |

#### toolbar

| 属性       | 说明 |
| ---------- | ---- |
| hasRefresh | 刷新 |
| hasFilter  | 过滤 |
| hasSort    | 排序 |
| hasAdd     | 新增 |
| hasEdit    | 编辑 |
| hasRemove  | 删除 |
| hasExport  | 导出 |
| hasImport  | 导入 |

