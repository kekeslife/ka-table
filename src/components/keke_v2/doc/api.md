# .Net

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

# JavaScript

## 插槽

| 插槽       | 说明                 |
| ---------- | -------------------- |
| 其它       | Antd vue table的插槽 |
| summaryBar | 汇总区               |
| bodyItem   | 单元格自定义内容渲染 |
| toolbar    | 工具栏扩展           |

## Expose

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

## Props

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

### toolbar

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

### column

| 属性           | 说明                                 |
| -------------- | ------------------------------------ |
| title: string  | 字段标题                             |
| dbInfo?:{}     | 数据类型配置                         |
| listInfo?:{}   | table显示配置                        |
| sortInfo?:{}   | 排序配置，默认所有字段都不开启排序   |
| filterInfo?:{} | 筛选配置，默认所有实体字段都开启排序 |
| exportInfo?:{} | 导出配置                             |
| importInfo?:{} | 导入配置                             |
| editorInfo?:{} | 数据编辑配置                         |

#### dbInfo

| 属性                                       | 说明         |
| ------------------------------------------ | ------------ |
| isPk?:boolean                              | 是否是主键   |
| dataType?: 'number' \|'**string**'\|'date' | 数据类型     |
| dateFormat?: string;                       | 日期格式遮罩 |

#### listInfo

| 属性                                                         | 说明                       |
| ------------------------------------------------------------ | -------------------------- |
| index: number                                                | 显示在第几列               |
| options?: KaEditorItemOption[] \| ((key: string) => KaEditorItemOption[]) | 按照value自动显示label内容 |
| title?                                                       | ant字段显示的标题          |
| width?                                                       | ant单元格宽度              |
| customCell?                                                  | ant设置单元格属性          |
| customHeaderCell?                                            | ant设置头部单元格属性      |
| customRender?                                                | ant生成复杂数据的渲染函数  |
| align?                                                       | ant对齐方式                |
| ellipsis?                                                    | ant超过宽度将自动省略      |
| attrs?: { [propName: string]: any }                          | 其它特性                   |

#### sortInfo

| 属性               | 说明                               |
| ------------------ | ---------------------------------- |
| index:number\|null | 设置number或null均代表开启字段排序 |

#### editorInfo

| 属性                                                         | 说明                                         |
| ------------------------------------------------------------ | -------------------------------------------- |
| index                                                        | 排序位置                                     |
| title?:string                                                | label标签                                    |
| width?:string                                                | 宽度                                         |
| position?:'inline'\|'cling'\|'**line**'                      | 位置方式(并列\|紧贴\|独行)                   |
| componentType?:'**input**'\|'textarea'\|'number'\|'date'\|'select' | 内置组件(文本框\|文本域\|数字\|日期\|选择器) |
| isPost?: boolean                                             | 是否是实体字段                               |
| display?: 'readonly' \|'hide'\|'**show**'                    | 显示方式(只读\|隐藏\|显示)                   |
| precision?: number                                           | ant数字字段的小数位                          |
| options?: KaEditorItemOption[]\|((key: string) => Promise<KaEditorItemOption[]>) | 选择器选项列表                               |
| selectMode?: 'multiple'\|'tags'\|'**combobox**'              | ant选择器模式                                |
| selectSplit?: string                                         | 多选分隔符                                   |
| rules?: Rule[]                                               | ant校验规则                                  |
| valueConverter?: (value: any) => Promise<any>                | 自定义值转换                                 |
| debounceDelay?: number                                       | 防抖延时                                     |
| customComponent?: Component                                  | 自定义组件                                   |
| onAfterChange?: (value: any, option?: any) => Promise<void>  | 值改变之后                                   |
| attrs?: { [propName: string]: any }                          | 其它特性                                     |

#### filterInfo

| 属性                                            | 说明                       |
| ----------------------------------------------- | -------------------------- |
| isFilter: boolean                               | 是否开启筛选               |
| options?:KaEditorItem['options']                | 选择器字段在筛选中的选择项 |
| width?                                          | 筛选中输入框的宽度         |
| valueConverter?: KaEditorItem['valueConverter'] | 值转换                     |
| attrs?: { [propName: string]: any }             | 其它特性                   |

#### exportInfo

| 属性             | 说明                                       |
| ---------------- | ------------------------------------------ |
| index: number    | 导出的字段在第几列                         |
| title?: string   | 导出的字段标题                             |
| formula?: string | 公式列。$row$行号, $col$列号，后台自动替换 |

#### importInfo

| 属性           | 说明               |
| -------------- | ------------------ |
| index: number  | 导入的字段在第几列 |
| title?: string | 导入的字段标题     |

