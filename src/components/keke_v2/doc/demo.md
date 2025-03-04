# 传统页面使用esm方式的问题

## 中文问题

ant-design-vue和dayjs，使用esm方式引用的话，对中文支持度不好，需要对这两个库单独打包

ant-design-vue：将'ant-design-vue/es/locale/zh_CN'用esm方式打包 zh-CN.js

dayjs：将dayjs和'dayjs/locale/zh-cn'用esm方式打包 day.js



html中引入文件并设置

```javascript
import zhCN from '/Scripts/antd4.1.2/zh-CN.js';
import dayjs from 'dayjs';

dayjs.locale('zh-cn');
```

```html
<ka-table :locale="zhCN"></ka-table>
```

## 插槽问题

传统方式的vue插槽会有驼峰的问题，可以通过下面方式解决

```html
<template #[slots.summary_bar]="{ summary }"></template>
```

```javascript
setup() {
	const slots = {
        summary_bar:'summaryBar',
    };
}
```



# 插槽 - 工具栏自定义扩展

在工具栏可以添加自己的扩展内容，如扩展自定义按钮

```html
<template #toolbar="{ dataSource }">
    <a-space-compact size="small">
        <a-tooltip><a-button @click="searchType('1')">修改程序</a-button></a-tooltip>
        <a-tooltip><a-button @click="searchType('2')">修改资料</a-button></a-tooltip>
        <a-tooltip><a-button @click="searchType('3')">账号管理</a-button></a-tooltip>
    </a-space-compact>
</template>
```

```javascript
const searchType = async(type) => {
    // 设置筛选条件
    $table.value.setFilters([{ key: 'type', opt: 'eq', val: type, bool: 'and' },]);
    // 读取数据
    await $table.value.reloadData();
}
```



# 插槽 - 自定义单元格显示内容

比如值转换、拼接字段

```html
<template #[slots.body_item]="{ column, text, record }">
    <!--后台实体字段[reqNo][filePath]，单元格显示[reqNo]的超链接指向[filePath]-->
    <a v-if="column.key === 'reqNo'" target="_blank" :href="record.filePath || null"><span :class="{grey:!record.filePath}">{{ record.reqNo }}</span></a>
    <!--后台实体字段[reqEmpNo][reqEmp]，单元格显示[reqEmpNo]-[reqEmp.empName]-->
    <span v-else-if="column.key === 'reqEmpNo'">{{record.reqEmpNo}}-{{record.reqEmp?.empName}}</span>
</template>
```



# 插槽 - 汇总栏

后台设置CustomerSummary可以通过EF汇总数据，前端可通过#summary插槽使用

```c#
table.CustomerSummary = t => new
{
    // Total为查询出的总行数，务必添加，不然会执行两次汇总sql
    Total = t.Count(),
    // 取得最大的编号
    MaxReqNo = t.Max(d => d.ReqNo)
};
```

```html
<template #[slots.summary_bar]="{ summary }">
    <a-table-summary-row>
        <a-table-summary-cell>Total</a-table-summary-cell>
        <a-table-summary-cell>
          <a-typography-text type="danger">{{ summary.maxReqNo }}</a-typography-text>
        </a-table-summary-cell>
    </a-table-summary-row>
</template>
```



# 编辑 - 前端临时字段

有时，前端需要用到临时的非实体字段

```javascript
tMode: {
    title: '模组',
    editorInfo: {
        // 设置为非实体字段
        isPost: false,
    },
},

// 读取数据之后
const onPostLoadData = (dataSource) => {
    // 将[reqNo]的前4位赋值给[tMode]
    for (const data of dataSource.records) {
        data.tMode = data.reqNo.substr(0, 4);
    }
    return { isSuccess: true };
}
```





# 编辑 - 新增、修改，不同显示

有时，你希望主键在新增时可输入，修改时不能输入

```javascript
const onBeforeAdd = () => {
    // 取得编辑项配置
    const editor = $table.value.getEditorObj();
    // [tMode]可输入
    editor.tMode.display = 'show';
    return { isSuccess: true };
}

const onBeforeEdit = () => {
    // 取得编辑项配置
    const editor = $table.value.getEditorObj();
    // [tMode]不可输入
    editor.tMode.display = 'readonly';
    return { isSuccess: true };
}
```



# 编辑 - 提交insert之前赋值

有时，你的实体主键是自动编号的，你需要在insert之前取到值

```javascript
const onPreAdd = async (dataSource) => {
    // 按照所填的内容后台编号
    const res = await axios.post('demandv2.ashx', Qs.stringify({
        actNo: 'getReqNo',
        mode: dataSource.editorRecord.tMode,
        ym: dataSource.editorRecord.reqDate.format('YYMM')
    }));
    if (!res.data.isSuccess) {
        return { isSuccess: false, message: res.data.message };
    }
    // 将后台的编号赋值给编辑项
    await $table.value.setEditorVal('reqNo', res.data.record, false, false);
    return { isSuccess: true };
}
```



# 编辑 - 提交insert之后显示内容

在通过onPreAdd取得编号之后，你需要在保存完成后显示其内容

```javascript
const onPostAdd = (editorRecord) => {
    // 显示提示框
    return { isSuccess: true,message:editorRecord.reqNo,alertType:'alert' };
}
```



# 编辑 - insert、update提交前

insert或update之前的触发器。比如可以做校验

```javascript
const onPreAddOrEdit = (dataSource) => {
	// 检验
    if (dataSource.editorRecord.reqHopeDate.isBefore(dataSource.editorRecord.reqDate)) {
        return { isSuccess: false, message: '希望完成日期 < 接单日期' };
    }
    return { isSuccess: true };
}
```

也可以在后台设置默认值

```c#
// 新增、修改前设置[时间]和[人员]默认值
table.BeforeInsert = (e) =>
{
    e.ModifyEmpNo = e.CreateEmpNo = new UserSession().User;
    e.ModifyDateTime = e.CreateDateTime = DateTime.Now;
};
table.BeforeUpdate = (e,old) =>
{
    e.ModifyEmpNo = new UserSession().User;
    e.ModifyDateTime = DateTime.Now;
};
```



# 编辑 - 初始化选择器选项

选择器的选项在mount时由后台提供

```javascript
setup(){
	// 供新增修改使用，后台返回的列表包含disabled:true属性，新增修改时不能选择
	const teamOptions = ref([]);
	// 供筛选使用，可选择所有的选项
	const allTeamOptions = ref([]);
	const initOptions = async () => {
        const res = await axios.post('demandv2.ashx', Qs.stringify({ actNo: 'getOptions' }));
        if (res.data.isSuccess) {
            teamOptions.value.push(...res.data.record.team);
            allTeamOptions.value.push(...res.data.record.team.map(l => ({ label: l.label, value: l.value })));
        };
    };
}
```

# 显示 - 固定列

```html
<ka-table :scroll="{x:'max-content'}"></ka-table>
```

```javascript
listInfo: {
	attrs:{fixed:'left'}
},
```



# 排序 - 自定义

```javascript
//js可通过setSorters设置{ key: '自定义', order: 'ascend', index: 1 }
```

```c#
// c#
table.CustomerOrders = [
	new KaTableCustomSort<ItDemand>{SortExpression = d => d.ReqDate, Key = "自定义", Order = "desc"},
];
```





# 导出 - 维护Excel

后台可维护产生的Excel

```c#
table.AfterExport = (worksheet, conditions, tot) =>
{
    // 按照前端的key取得字段的列索引
    var colIndex = Array.FindIndex(conditions, c => c.Key == "status");
    if (colIndex == -1) return;
    // 列号从1开始
    colIndex++;

    var status = new Dictionary<string, string>
    {
        { "0", "作废" },
        { "1", "登记" },
        { "2", "完成" },
        { "3", "结案" },
    };

    // 转换枚举值
    for (var i = 0; i < tot; i++)
    {
        if (status.TryGetValue(worksheet.Cells[i+2, colIndex].Value?.ToString() ?? "", out var val))
        {
            worksheet.Cells[i+2, colIndex].Value = val;
        }
    }
};
```

