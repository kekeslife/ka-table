import type { TableProps } from 'ant-design-vue';
import { Rule } from 'ant-design-vue/es/form';
import { ColumnType, tableProps } from 'ant-design-vue/es/table';
import { Key, SortOrder } from 'ant-design-vue/es/table/interface';
import { PropType, customRef } from 'vue';

/** props */
// type KaTableProps = TableProps & {
//     tableTitle?: string,
//     toolbar: KaTablePropsToolbar,
//     language: KaTableLang,
// }

/** 事件返回结果 */
type KaTableEventResult = {
    isSuccess: boolean;
    message?: string;
}

type KaTableEventHandle = (record: KaTableRecord) => Promise<KaTableEventResult>;

/** 表格状态 */
type KaTableStatus = 'List' | 'Add' | 'Edit' | 'Delete' | 'Import' | 'Export' | null;

/** 开启内置工具栏 */
type KaTablePropsToolbar = {
    hasRefresh: boolean,
    hasSearch: boolean,
    hasSort: boolean,
    hasAdd: boolean,
    hasEdit: boolean,
    hasDelete: boolean,
    hasExport: boolean,
    hasImport: boolean,
}

/** 多语言 */
type KaTableLang = {
    toolbarRefresh: string,
    toolbarSearch: string,
    toolbarSort: string,
    toolbarEdit: string,
    toolbarDelete: string,
    toolbarAdd: string,
    toolbarImport: string,
    toolbarExport: string,
    noChange: string,
    refreshError: string,
    loadError: string,
    addError: string,
    addSuccess: string,
    editSuccess: string,
    confirm: string,
    reset: string,
    cancel: string,
    submit: string,
}
/** 所有数据 */
type KaTableRecord = {
    /** 页面上加载的数据 */
    records: readonly KaTableRowRecord[],
    /** 当前选中的行索引 */
    activeIndex: number | null,
    /** 编辑中的数据 */
    editRecord: KaTableRowRecord,
    /** 分页信息 */
    page: KaTablePage,
}
/** 记录行 */
type KaTableRowRecord = {
    isActive?: boolean,
    [propName: string]: any,
}

/** 分页 */
type KaTablePage = {
    current?: number,
    pageSize?: number,
    total?: number
}

type KaTableSortItem = {
    index: number,
    order: SortOrder,
}

/** post查询参数 */
type KaTableSearchPar = {
    pageSize: number;
    pageNum: number;
    sortConditions?: KaTableSortCondition[];
    whereConditions?: KaTableWhereCondition[];
}

type KaTableSortCondition = {
    col: string;
    order: 'ascend' | 'descend';
}

type KaTableWhereCondition = {
    col?: string;
    opt?: 'eq' | 'neq' | 'gte' | 'ge' | 'lt' | 'lte' | 'beg' | 'end' | 'like' | 'in' | 'nu' | 'nnu';
    val?: any;
    bool: 'and' | 'or';
    children?: KaTableWhereCondition[];
}

/** 服务器返回查询 */
type KaTableResponse = {
    isSuccess: boolean;
    records: any[];
    message: string | null;
    total: number;
    summary: number[] | null;
}

/** 选择组件项目 */
type KaTableOptionItem = {
    value: any;
    label: string;
};

/** 编辑框排列 */
type KaTableEditCol = {
    col: KaTableCol<any>;
    children?: KaTableEditCol[];
}

/** 数据类型 */
type KaTableColType = 'number' | 'string' | 'date';

type KaTableInputType = KaTableColType | 'select' | 'textarea';

/** 字段属性 antd.ColumnType*/
type KaTableCol<T> = ColumnType<T> & {
    initSorter?: KaTableSortItem;
    isSort?: boolean;
    isFilt?: boolean;
    isImport?: boolean;
    isExport?: boolean;
    isRequired?: boolean;
    dbInfo?: {
        isPk?: boolean;
        colType?: 'number' | 'string' | 'date';
        dateFormat?: string;
        /** 数据长度(3.2代表3整数,2小数) */
        colLength?: number;
    };
    listInfo?: {
        index: number;
        title?: string;
        width?: number | string;
        options?: KaTableOptionItem[] | ((record: KaTableRecord) => Promise<KaTableOptionItem[]>);
        // onBeforeList?: ((text:any, record: KaTableRecord) => Promise<string>);
    },
    editInfo?: {
        index: number;
        title?: string;
        width?: number | string;
        position?: 'inline' | 'cling' | 'line';
        inputType?: KaTableInputType;
        isPost?: boolean;
        display?: 'readonly' | 'hide' | 'show';
        options?: KaTableOptionItem[] | ((record: KaTableRecord, key: string) => Promise<KaTableOptionItem[]>);
        onAfterChange?: KaTableEventHandle;
        attrs?: { [propName: string]: any },
        rules?: Rule[],
    },

    // initWhereConditions?:KaTableWhereCondition[],
    // frozenWhereConditions?:KaTableWhereCondition[],
    //initSortConditions
    //frozenSortConditions
    //defaultInsertConditions
    //defaultUpdateConditions
}
/** 字段属性 antd.ColumnGroupType*/
interface KaTableGroupCol<T> extends Omit<KaTableCol<T>, 'dataIndex'> {
    children: KaTableCol<T>;
}
/** 字段属性 */
type KaTableCols<T> = KaTableCol<T>[];//| KaTableGroupCol<T>[];

export type { KaTableRowRecord, KaTablePage, KaTableSearchPar, KaTableWhereCondition, KaTableSortCondition, KaTableCol, KaTableEditCol, KaTableCols, KaTableInputType, KaTableOptionItem, KaTableRecord, KaTableEventHandle, KaTableResponse, KaTableSortItem, KaTableStatus };

/** 防抖ref */
export function useDebouncedRef<T>(value: T, delay = 200) {
    let timeout: number;
    return customRef((track, trigger) => {
        return {
            get() {
                track()
                return value
            },
            set(newValue) {
                clearTimeout(timeout)
                timeout = setTimeout(() => {
                    value = newValue
                    trigger()
                }, delay)
            }
        }
    })
}

/** 判断类型 */
export function isArray(obj: any) {
    if (obj === undefined) return false;
    return obj.constructor === Array;
}
/** 判断类型 */
export function isObject(obj: any) {
    if (obj === undefined) return false;
    return obj.constructor === Object;
}
/** 判断类型 */
export function isFunction(obj: any) {
    if (obj === undefined) return false;
    return obj.constructor === Function || obj instanceof Function;
}
/** 判断类型 */
export function isBool(obj: any) {
    if (obj === undefined) return false;
    return obj.constructor === Boolean;
}
/** 判断类型 */
export function isNumber(obj: any) {
    if (obj === undefined) return false;
    return obj.constructor === Number;
}
/** 判断类型 */
export function isString(obj: any) {
    if (obj === undefined) return false;
    return obj.constructor === String;
}

export const kaTableProps = () => ({
    ...tableProps(),
    /** 字段配置 */
    columns: { type: Object as PropType<KaTableCols<any>>, required: true, default: [] },
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
    /** 新增窗口宽度 */
    editDialogWidth: { type: [Number, String], default: 720 },
    /** 修改标题 */
    editTitle: { type: String, default: '' },

    initWhereConditions: { type: Array as PropType<KaTableWhereCondition[]>, default: null },
    frozenWhereConditions: { type: Array as PropType<KaTableWhereCondition[]>, default: null },

    /** 多国语言 */
    language: {
        type: Object as PropType<KaTableLang>,
        default: () => ({
            toolbarRefresh: '刷新',
            toolbarSearch: '查询',
            toolbarSort: '排序',
            toolbarEdit: '编辑',
            toolbarDelete: '删除',
            toolbarAdd: '新增',
            toolbarExport: '导出',
            toolbarImport: '导入',
            noChange: '没啥需要更新',
            refreshError: '刷新失败',
            addError: '新增失败',
            addSuccess: '新增成功',
            editSuccess: '修改成功',
            loadError: '读取数据失败',
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
            hasSearch: true,
            hasSort: true,
            hasAdd: true,
            hasEdit: true,
            hasDelete: true,
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
    onBeforeSearch: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击查询按钮，显示查询框之后 */
    onAfterSearch: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交查询之前 */
    onPreSearch: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 查询之后 */
    onPostSearch: {
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
    onBeforeDelete: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 点击删除按钮，确认之后 */
    onAfterDelete: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** 提交Delete之前 */
    onPreDelete: {
        type: Function as PropType<KaTableEventHandle>
    },
    /** Delete之后 */
    onPostDelete: {
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


