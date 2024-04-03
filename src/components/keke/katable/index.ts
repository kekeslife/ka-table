import { SizeType } from "ant-design-vue/es/config-provider";
import { NamePath, Rule } from "ant-design-vue/es/form/interface";
import { TableProps, tableProps } from "ant-design-vue/es/table";
import { ColumnType, SortOrder } from "ant-design-vue/es/table/interface";
import { Dayjs } from "dayjs";
import { Prop, PropType } from "vue";

/** 表格状态 */
export type KaTableStatus = 'List' | 'Sort' | 'Filter' | 'Add' | 'Edit' | 'Remove' | 'Import' | 'Export' | null;

/** 开启内置工具栏 */
export type KaTablePropsToolbar = {
    hasRefresh?: boolean,
    hasFilter?: boolean,
    hasSort?: boolean,
    hasAdd?: boolean,
    hasEdit?: boolean,
    hasRemove?: boolean,
    hasExport?: boolean,
    hasImport?: boolean,
}


/** 多语言 */
export type KaTableLang = {
    toolbarRefresh: string,
    toolbarFilter: string,
    toolbarSort: string,
    toolbarEdit: string,
    toolbarRemove: string,
    toolbarAdd: string,
    toolbarImport: string,
    toolbarExport: string,
    noChange: string,
    refreshError: string,
    loadError: string,
    exportError: string,
    addError: string,
    addSuccess: string,
    editSuccess: string,
    selectRow: string,
    confirm: string,
    reset: string,
    cancel: string,
    submit: string,
}

/** 数据类型 */
export type KaTableColType = 'number' | 'string' | 'date';
/** 数据框类型 */
export type KaTableInputType = KaTableColType | 'select' | 'textarea';
/** 字段 */
export type KaTableCol = {
    title: string;
    /** 'docFlow.status' */
    key?: string;
    /**标记是否是col */
    _katableIsCol?: boolean;
    dbInfo?: {
        isPk?: boolean;
        dataType?: 'number' | 'string' | 'date';
        dateFormat?: string;
        /** 数据长度(3.2代表3整数,2小数) */
        colLength?: number;
    };
    listInfo: {
        /** 显示顺序 */
        index: number;
        /** 值自动转换显示内容 */
        options?: KaTableOptionItem[] | ((record: KaTableRowRecord) => string);
        /** 自定义显示内容函数 */
        //onBeforeList?: ((dataSource: KaTableDataSource) => Promise<string>);
        // ant
        title?: ColumnType<any>['title'];
        width?: ColumnType<any>['width'];
        align?: ColumnType<any>['align'];
        customCell?: ColumnType<any>['customCell'];
        customHeaderCell?: ColumnType<any>['customHeaderCell'];
        customRender?: ColumnType<any>['customRender'];
        ellipsis?: ColumnType<any>['ellipsis'];
        fixed?: ColumnType<any>['fixed'];
        maxWidth?: ColumnType<any>['maxWidth'];
        minWidth?: ColumnType<any>['minWidth'];
        resizable?: ColumnType<any>['resizable'];
        responsive?: ColumnType<any>['responsive'];
        // rowScope?:ColumnType<any>['rowScope'];
    },
    sortInfo?: {
        /** 排序顺序 */
        index: number | null;
        /** 排序方式 */
        order: ColumnType<any>['sortOrder'];

        // ant
        showSorterTooltip?: ColumnType<any>['showSorterTooltip'];
        // sortDirections?:ColumnType<any>['sortDirections'];
    },
    filterInfo?: {
        isFilter?: boolean;
        options?: KaTableOptionItem[];
    },
    exportInfo?: {
        index: number | null;
        title?: string;
        /** 自定义excel公式 */
        formula?: string;
    },
    editorInfo?: {
        index: number;
        title?: string;
        width?: number | string;
        position?: 'inline' | 'cling' | 'line';
        inputType?: KaTableInputType;
        isPost?: boolean;
        // isRequired?: boolean;
        display?: 'readonly' | 'hide' | 'show';
        options?: KaTableOptionItem[] | ((editRecord: KaTableRowRecord, key: string) => Promise<KaTableOptionItem[]>);
        onAfterChange?: (editRecord: KaTableRowRecord,value:any,option?:any)=> Promise<void>;
        selectMode?:'multiple' | 'tags' | 'combobox';
        selectSplit?:string;
        attrs?: { [propName: string]: any },
        rules?: Rule[],
    },
}
/** 字段集合
 * {
 *  code:{
 *      title:'',
 *      _katableIsCol:true,
 *  },
 *  docFlow:{
 *      code:{
 *          title:'',
 *          _katableIsCol:true,
 *      }
 *  }
 * }
 */
export type KaTableCols = {
    [key: string]: KaTableCol | { [key: string]: KaTableCol };
}

/** 数据源 */
export type KaTableDataSource = {
    records: KaTableRowRecord[];
    activeIndex: number | null;
    readonly curRecord: KaTableRowRecord | null;
    editRecord: KaTableRowRecord;
}
/** 数据行 */
export type KaTableRowRecord = { [key: string]: string | number | boolean | null | Dayjs | KaTableRowRecord }

/** 分页 */
export type KaTablePage = {
    current?: number,
    pageSize?: number,
    total?: number
}
/** 排序 */
export type KaTableSortOrder = {
    index: number | null,
    order: SortOrder,
    col: string,
}

/** post查询参数 */
export type KaTableSearchPar = {
    pageSize: number;
    pageNum: number;
    sortConditions?: KaTableSortCondition[];
    whereConditions?: KaTableWhereCondition[];
}
/** post导出参数 */
export type KaTableExportPar = KaTableSearchPar & {
    cols: { col: string; title: string; formula?: string,dateFormat?:string }[];
    fileName: string;
}

export type KaTableSortCondition = {
    col: string;
    order: 'ascend' | 'descend';
}

export type KaTableWhereCondition = {
    col?: string;
    opt?: 'eq' | 'neq' | 'gte' | 'ge' | 'lt' | 'lte' | 'beg' | 'end' | 'like' | 'in' | 'nu' | 'nnu';
    val?: any;
    bool: 'and' | 'or';
    children?: KaTableWhereCondition[];
}

/** 服务器返回 */
export type KaTableResponse = {
    isSuccess: boolean;
    message: string | null;
}
/** 服务器返回查询 */
export type KaTableSearchResponse = KaTableResponse & {
    records: any[];
    total: number;
    summary: number[] | null;
}

/** 事件返回结果 */
export type KaTableEventResult = {
    isSuccess: boolean;
    message?: string;
}

/** 事件 */
export type KaTableEventHandle = (record: KaTableDataSource) => Promise<KaTableEventResult>;

/** select选项 */
export type KaTableOptionItem = {
    label?: string;
    value: any;
    [key:string]:any;
}

export const kaTableProps = () => ({
    //...tableProps(),
    /** 尺寸 */
    size: { type: String as PropType<SizeType>, default: 'small' },
    /** 字段配置 */
    columns: { type: Object as PropType<KaTableCols>, required: true, default: [] },
    /** 表格标题 */
    tableTitle: { type: String },
    /** 主题颜色 */
    theme: { type: String },
    /** API地址 */
    url: { type: String, required: true, default: '' },
    /** 每页显示条数，默认10条，设置0为不分页 */
    pageSize: { type: Number, default: 10 },
    /** 初始化后自动加载数据 */
    autoLoad: { type: Boolean, default: true },
    /** 读取数据后自动选中第一行 */
    autoSelect: { type: Boolean, default: true },
    /** 是否调试模式 */
    isDebug: { type: Boolean, default: false },
    /** 新增标题 */
    addTitle: { type: String, default: '' },
    /** 抽屉窗口宽度 */
    drawWidth: { type: [Number, String], default: 720 },
    /** 抽屉提交按钮文字 */
    drawSubmitTitle: { type: String, default: '' },
    /** 修改标题 */
    editTitle: { type: String, default: '' },
    /** 排序标题 */
    sortTitle: { type: String, default: '' },
    /** 筛选标题 */
    filterTitle: { type: String, default: '' },
    /** 导出文件名 */
    exportFileName: { type: Function as PropType<() => string> },

    initFilterConditions: { type: Array as PropType<KaTableWhereCondition[]>, default: null },
    frozenFilterConditions: { type: Array as PropType<KaTableWhereCondition[]>, default: null },

    /** 多国语言 */
    language: {
        type: Object as PropType<KaTableLang>,
        default: () => ({
            toolbarRefresh: '刷新',
            toolbarFilter: '筛选',
            toolbarSort: '排序',
            toolbarEdit: '编辑',
            toolbarRemove: '删除',
            toolbarAdd: '新增',
            toolbarExport: '导出',
            toolbarImport: '导入',
            noChange: '没啥需要更新',
            refreshError: '刷新失败',
            exportError: '导出失败',
            addError: '新增失败',
            addSuccess: '新增成功',
            editSuccess: '修改成功',
            loadError: '读取数据失败',
            selectRow:'请先选中一行',
            confirm: '确定',
            submit: '提交',
            reset: '重置',
            cancel: '取消',
        })
    },
    /** 内置工具栏启用 */
    toolbar: {
        type: Object as PropType<KaTablePropsToolbar>,
        default: () => ({
            hasRefresh: true,
            hasFilter: true,
            hasSort: true,
            hasAdd: true,
            hasEdit: true,
            hasRemove: true,
            hasExport: true,
            hasImport: true,
        })
    },

    /** 点击刷新按钮，刷新数据之前 */
    onBeforeRefresh: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 刷新数据之后 */
    onPostRefresh: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 读取数据之后 */
    onPostLoadData: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击查询按钮，显示查询框之前 */
    onBeforeFilter: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击查询按钮，显示查询框之后 */
    onAfterFilter: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交查询之前 */
    onPreFilter: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 查询之后 */
    onPostFilter: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击排序按钮，显示排序框之前 */
    onBeforeSort: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击排序按钮，显示排序框之后 */
    onAfterSort: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交排序之前 */
    onPreSort: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 排序之后 */
    onPostSort: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击修改按钮，数据填入之前，用于判断是否可以修改 */
    onBeforeEdit: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击修改按钮，数据填入之后，默认值可在这里设置 */
    onAfterEdit: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交Update之前 */
    onPreEdit: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** Update之后 */
    onPostEdit: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击新增按钮，数据填入之前 */
    onBeforeAdd: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击新增按钮，数据填入之后 */
    onAfterAdd: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交Insert之前 */
    onPreAdd: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** Insert之后 */
    onPostAdd: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击删除按钮，显示确认之前 */
    onBeforeRemove: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击删除按钮，确认之后 */
    onAfterRemove: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交Remove之前 */
    onPreRemove: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** Remove之后 */
    onPostRemove: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击导出按钮，显示确认之前 */
    onBeforeExport: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击导出按钮，确认之后 */
    onAfterExport: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交Export之前 */
    onPreExport: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** Export之后 */
    onPostExport: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击导入按钮，选择文件之前 */
    onBeforeImportFile: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击导入按钮，选择文件之后 */
    onAfterImportFile: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 解析Excel文件之前 */
    onPreImportFile: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 解析Excel文件之后 */
    onPostImportFile: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交导入数据之前 */
    onPreImport: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 导入数据之后 */
    onPostImport: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击分页按钮，提交分页之前 */
    onPrePage: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交分页之后 */
    onPostPage: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击行之后 */
    onAfterRowClick: {
        type: Function as PropType<KaTableEventHandle>
    },
});
