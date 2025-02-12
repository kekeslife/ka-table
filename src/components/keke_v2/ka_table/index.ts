import { SizeType } from 'ant-design-vue/es/config-provider';
import { Rule } from 'ant-design-vue/es/form/interface';
import { ColumnType, SortOrder } from 'ant-design-vue/es/table/interface';
import { PropType } from 'vue';
import { KaEditorItem, KaEditorItemOption } from '../ka_editor';
import { SelectProps, TableColumnProps,TableProps } from 'ant-design-vue';
import { KaSorterCondition } from '../ka_sorter';
import { KaFilterCondition } from '../ka_filter';
import { type Locale } from 'ant-design-vue/es/locale';

/** 表格状态 */
export type KaTableStatus = 'List' | 'Sort' | 'Filter' | 'Add' | 'Edit' | 'Remove' | 'Import' | 'Export' | null;

/** 开启内置工具栏 */
export type KaTablePropsToolbar = {
	hasRefresh?: boolean;
	hasFilter?: boolean;
	hasSort?: boolean;
	hasAdd?: boolean;
	hasEdit?: boolean;
	hasRemove?: boolean;
	hasExport?: boolean;
	hasImport?: boolean;
};

/** 多语言 */
export type KaTableLang = {
	toolbarRefresh: string;
	toolbarFilter: string;
	toolbarSort: string;
	toolbarEdit: string;
	toolbarRemove: string;
	toolbarAdd: string;
	toolbarImport: string;
	toolbarExport: string;
	noChange: string;
	refreshError: string;
	loadError: string;
	exportError: string;
	addError: string;
	editError: string;
	removeError: string;
	importError: string;
	templateError: string;
	addSuccess: string;
	editSuccess: string;
	removeConfirm:string;
	removeSuccess: string;
	importSuccess: string;
	importNoFileError: string;
	importFileError: string;
	selectRow: string;
	confirm: string;
	clear: string;
	reset: string;
	cancel: string;
	submit: string;
	summaryTotal: string;
	all: string;
	curPage: string;
	and:string;
	or:string;
	equal:string;
	notEqual:string;
	greaterThan:string;
	greaterThanOrEqual:string;
	lessThan:string;
	lessThanOrEqual:string;
	beginWith:string;
	endWith:string;
	contain:string;
	empty:string;
	notEmpty:string;
};

/** 字段 */
export type KaTableCol = {
	/** 必需，用于初始化区分是字段还是子对象。 */
	title: string;
	/** 字段名，自动赋值。 如：'docFlow.status' */
	key?: string;
	/** 标记是否是字段 */
	_katableIsCol?: boolean;
	dbInfo?: {
		isPk?: boolean;
		dataType?: 'number' | 'string' | 'date';
		dateFormat?: string;
		/** 数据长度(3.2代表3整数,2小数) */
		// colLength?: number;
	};
	listInfo?: {
		/** 显示顺序 */
		index: number;
		/** 值自动转换显示内容 */
		// options?: KaTableOptionItem[] | ((record: KaTableRowRecord) => string);
		options?: KaEditorItemOption[] | ((key: string) => KaEditorItemOption[]);
		/** 自定义显示内容函数 */
		//onBeforeList?: ((dataSource: KaTableDataSource) => Promise<string>);
		// ant
		title?: ColumnType<any>['title'];
		width?: ColumnType<any>['width'];
		customCell?: ColumnType<any>['customCell'];
		customHeaderCell?: ColumnType<any>['customHeaderCell'];
		customRender?: ColumnType<any>['customRender'];
		align?: ColumnType<any>['align'];
		ellipsis?: ColumnType<any>['ellipsis'];
		// fixed?: ColumnType<any>['fixed'];
		// maxWidth?: ColumnType<any>['maxWidth'];
		// minWidth?: ColumnType<any>['minWidth'];
		// resizable?: ColumnType<any>['resizable'];
		// responsive?: ColumnType<any>['responsive'];
		/** 自定义attr */
		attrs?: { [propName: string]: any };
		// rowScope?:ColumnType<any>['rowScope'];
	};
	sortInfo?: {
		/** 排序顺序 */
		index: number | null;
		/** 排序方式 */
		// order: ColumnType<any>['sortOrder'];
		/** 自定义attr */
		// attrs?: { [propName: string]: any },
		// ant
		//showSorterTooltip?: ColumnType<any>['showSorterTooltip'];
		// sortDirections?:ColumnType<any>['sortDirections'];
	};
	filterInfo?: {
		isFilter: boolean;
		options?: KaEditorItem['options'];
		width?:ColumnType<any>['width'];
		title?:string;
		/** 自定义值转换 */
		valueConverter?: KaEditorItem['valueConverter'];
		/** 自定义attr */
		attrs?: { [propName: string]: any };
	};
	exportInfo?: {
		index: number | null;
		title?: string;
		/** 自定义excel公式 */
		formula?: string;
	};
	importInfo?: {
		index: number | null;
		title?: string;
	};
	editorInfo?: {
		/** 编辑框排序 */
		index: KaEditorItem['index'];
		/** 编辑框标签 */
		title?: KaEditorItem['title'];
		/** 编辑框宽度 */
		width?: KaEditorItem['width'];
		/** 显示位置 'inline' | 'cling' | 'line'*/
		position?: KaEditorItem['position'];
		/** 内置组件 'input' | 'textarea' | 'number' | 'date' | 'select' */
		componentType?: KaEditorItem['componentType'];
		/** 是否提交 */
		isPost?: KaEditorItem['isPost'];
		// isRequired?: boolean;
		display?: KaEditorItem['display'];
		/** ant select选项 */
		options?: KaEditorItem['options'];
		/** 在值改变之后 */
		onAfterChange?: KaEditorItem['onAfterChange'];
		/** ant select mode */
		selectMode?: SelectProps['mode'];
		/** select多选分隔符 */
		selectSplit?: string;
		/** 自定义attr */
		attrs?: { [propName: string]: any };
		/** ant formItem 验证 */
		rules?: Rule[];
		/** 自定义值转换 */
		valueConverter?: KaEditorItem['valueConverter'];
		/** 自定义组件 */
		customComponent?: KaEditorItem['customComponent'];
		/** 值改变触发onAfterChange防抖延时 */
		debounceDelay?: KaEditorItem['debounceDelay'];
		/** 小数精度 */
		precision?: number;
	};
	// editorInfo?:Omit<KaEditorItem,'showTime'|'_value'|'_key'>
};
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
};

/** 数据源 */
export type KaTableDataSource = {
	records: KaTableRowRecord[];
	activeIndex: number | null;
	readonly curRecord: KaTableRowRecord | null;
	summary:{[key: string]: any} | null;
};
/** 数据行 */
export type KaTableRowRecord = { [key: string]: any };

/** 分页 */
export type KaTablePage = {
	current?: number;
	pageSize?: number;
	total?: number;
};
/** 显示 */
export type KaTableListCol = TableColumnProps & {
	index: number;
	/** 值自动转换显示内容 */
	options?: KaEditorItemOption[] | ((key: string) => KaEditorItemOption[]);
};
/** 排序 */
export type KaTableSortOrder = {
	index: number | null;
	order: SortOrder;
	col: string;
};

export type KaTableImportCol = TableColumnProps & {
	index: number;
};

/** post查询参数 */
export type KaTableSearchPar = {
	pageSize: number;
	pageNum: number;
	sortConditions?: KaSorterCondition[];
	whereConditions?: KaFilterCondition[];
};

/** post导出参数 */
export type KaTableExportPar = KaTableSearchPar & {
	cols: { key: string; title: string; formula?: string; dateFormat?: string; index: number }[];
	fileName: string;
};

export type KaTableImportPar = {
	cols: { key: string; title: string }[];
	file: File;
};

export type KaTableSortCondition = {
	col: string;
	order: 'ascend' | 'descend';
};

export type KaTableWhereCondition = {
	col?: string;
	opt?: 'eq' | 'neq' | 'gte' | 'ge' | 'lt' | 'lte' | 'beg' | 'end' | 'like' | 'in' | 'nu' | 'nnu';
	val?: any;
	bool: 'and' | 'or';
	children?: KaTableWhereCondition[];
};

/** 服务器返回 */
export type KaTableResponse = {
	isSuccess: boolean;
	message: string | null;
};
export type KaTableResponseRecord = KaTableResponse & {
	record: any;
};
export type KaTableResponseRecords = KaTableResponse & {
	records: any[];
};
/** 服务器返回查询 */
export type KaTableSearchResponse = KaTableResponseRecords & {
	total: number;
	summary?:{[key:string]:any};
};
/** 服务器返回导入 */
export type KaTableImportFileResponse = KaTableResponse & {
	records: any[];
};

/** 事件返回结果 */
export type KaTableEventResult = {
	isSuccess: boolean;
	message?: string;
	alertType?: 'alert' | 'confirm';
};

/** 事件 */
export type KaTableEventHandle = (data: any) => Promise<KaTableEventResult>;

/** select选项 */
export type KaTableOptionItem = {
	label?: string;
	value: any;
	[key: string]: any;
};

export const kaTableProps = () => ({
	//...tableProps(),
	locale: { type: Object as PropType<Locale> },
	/** 尺寸 */
	size: { type: String as PropType<SizeType>, default: 'small' },
	/** ant table scroll */
	scroll: { type: Object as PropType<TableProps['scroll']> },
	/** 字段配置 */
	columns: { type: Object as PropType<KaTableCols>, required: true },
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
	/** 刷新标题 */
	refreshTitle: { type: String, default: '' },
	/** 抽屉窗口宽度 */
	drawWidth: { type: [Number, String], default: 720 },
	/** 抽屉提交按钮文字 */
	drawSubmitTitle: { type: String, default: '' },
	/** 抽屉清空按钮文字 */
	drawClearTitle: { type: String, default: '' },
	/** 修改标题 */
	editTitle: { type: String, default: '' },
	/** 排序标题 */
	sortTitle: { type: String, default: '' },
	/** 筛选标题 */
	filterTitle: { type: String, default: '' },
	/** 删除标题 */
	removeTitle: { type: String, default: '' },
	/** 导入标题 */
	importTitle: { type: String, default: '' },
	/** 导出标题 */
	exportTitle: { type: String, default: '' },
	/** 导出文件名 */
	exportFileName: { type: Function as PropType<() => string> },

	initFilterConditions: { type: Array as PropType<KaFilterCondition[]>, default: [] },
	frozenFilterConditions: { type: Array as PropType<KaFilterCondition[]>, default: [] },
	initSorterConditions: { type: Array as PropType<KaSorterCondition[]>, default: [] },

	/** 多国语言 */
	language: {
		type: Object as PropType<KaTableLang>,
		default: {},
	},
	/** 内置语言 */
	defaultLang:{
		type:String as PropType<'cn' | 'en'>,
		default: 'cn',
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
			hasExport: false,
			hasImport: false,
		}),
	},

	/** 点击刷新按钮，刷新数据之前 */
	onBeforeRefresh: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 刷新数据之后 */
	onPostRefresh: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 读取数据之后 */
	onPostLoadData: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击筛选按钮，显示筛选框之前 */
	onBeforeFilter: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击筛选按钮，显示筛选框之后 */
	onAfterFilter: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交筛选之前 */
	onPreFilter: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交筛选之后 */
	onPostFilter: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击排序按钮，显示排序框之前 */
	onBeforeSort: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击排序按钮，显示排序框之后 */
	onAfterSort: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交排序之前 */
	onPreSort: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交排序之后 */
	onPostSort: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击修改按钮，数据填入之前，用于判断是否可以修改 */
	onBeforeEdit: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击修改按钮，数据填入之后，默认值可在这里设置 */
	onAfterEdit: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Update之前 */
	onPreEdit: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Update之后 */
	onPostEdit: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击新增按钮，数据填入之前 */
	onBeforeAdd: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击新增按钮，数据填入之后 */
	onAfterAdd: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Insert之前 */
	onPreAdd: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Insert之后 */
	onPostAdd: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Insert或Update之前 */
	onPreAddOrEdit: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Insert或Update之后 */
	onPostAddOrEdit: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Insert或Update之前 */
	onPreEditOrRemove: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Remove之前 */
	onPreRemove: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Remove之后 */
	onPostRemove: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击导出按钮，显示确认之前 */
	onBeforeExport: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击导出按钮，确认之后 */
	onAfterExport: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Export之前 */
	onPreExport: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交Export之后 */
	onPostExport: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击导入按钮，选择文件之前 */
	onBeforeImportFile: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击导入按钮，选择文件之后 */
	onAfterImportFile: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交解析Excel文件之前 */
	onPreImportFile: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交解析Excel文件之后 */
	onPostImportFile: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交导入数据之前 */
	onPreImport: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交导入数据之后 */
	onPostImport: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击分页按钮，提交分页之前 */
	onPrePage: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 提交分页之后 */
	onPostPage: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 点击行之后 */
	onAfterRowClick: {
		type: Function as PropType<KaTableEventHandle>,
	},
	/** 双击行之后 */
	onAfterRowDbClick: {
		type: Function as PropType<KaTableEventHandle>,
	},
});
