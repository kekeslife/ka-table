<template>
	<a-config-provider
		:component-size="props.size"
		:locale="props.locale"
		:theme="
			props.theme
				? {
						token: {
							colorPrimary: props.theme,
							colorBorderSecondary: props.theme,
							colorFillAlter: colsColor,
							colorFillSecondary: colsColor,
							colorBgContainerDisabled: '#F0F0F0',
						},
				  }
				: {}
		"
	>
		<a-table
			class="ka-table"
			bordered
			:custom-row="rowEvent"
			:row-class-name="rowClass"
			:pagination="pagination.pageSize === 0 ? false : pagination"
			@change="onAntTableChange"
			:loading="loading.list"
			:data-source="dataSource.records"
			:showSorterTooltip="false"
			:columns="antCols"
		>
			<template v-for="(_v, k) in $slots" v-slot:[k] :key="k">
				<slot :name="k"></slot>
			</template>
			<!-- 汇总栏 -->
			<template #summary>
				<slot name="summaryBar" :summary="dataSource.summary"></slot>
			</template>
			<!-- 自定义单元格 -->
			<template #bodyCell="{ text, column, index, record }">
				<slot name="bodyItem" :column="column" :index="index" :text="text" :record="record">
					{{ formatItem(text, column, record) }}
				</slot>
			</template>

			<template #title>
				<!-- 标题栏 -->
				<a-flex class="ka-table-title" justify="space-between" align="center">
					<!-- 标题 -->
					<a-typography-title :level="4" style="margin: 0">{{ props.tableTitle }}</a-typography-title>
					<!-- 工具栏 -->
					<div>
						<ka-toolbar
							:refresh="{
								isShow: props.toolbar.hasRefresh,
								title: props.refreshTitle || props.language.toolbarRefresh,
								onClick: onToolbarRefresh,
							}"
							:filter="{
								isShow: props.toolbar.hasFilter,
								title: props.filterTitle || props.language.toolbarFilter,
								isActive: isFiltered,
								onClick: onToolbarFilter,
							}"
							:export="{
								isShow: props.toolbar.hasExport,
								title: props.exportTitle || props.language.toolbarExport,
								onExportAll(_e) {
									onToolbarExport(true);
								},
								onExportPage(_e) {
									onToolbarExport(false);
								},
							}"
							:sort="{
								isActive: isSorted,
								isShow: props.toolbar.hasSort,
								title: props.sortTitle || props.language.toolbarSort,
								onClick: onToolbarSort,
							}"
							:add="{
								isShow: props.toolbar.hasAdd,
								title: props.addTitle || props.language.toolbarAdd,
								onClick: onToolbarAdd,
							}"
							:import="{
								isShow: props.toolbar.hasImport,
								title: props.importTitle || props.language.toolbarImport,
								beforeUpload: uploadFile,
								downloadTemplate: downloadTemplate,
							}"
							:edit="{
								isDisabled: dataSource.activeIndex == null,
								isShow: props.toolbar.hasEdit,
								title: props.editTitle || props.language.toolbarEdit,
								onClick: onToolbarEdit,
							}"
							:remove="{
								isShow: props.toolbar.hasRemove,
								title: props.removeTitle || props.language.toolbarRemove,
								isDisabled: dataSource.activeIndex == null,
								onClick: onToolbarRemove,
							}"
							:activate-color="primaryColor"
						>
							<template #toolbar><slot name="toolbar" :data-source="dataSource"></slot></template>
						</ka-toolbar>
					</div>
				</a-flex>
			</template>
			<!-- 筛选框 -->
			<template #customFilterDropdown="{ setSelectedKeys, selectedKeys, confirm, clearFilters, column }">
				<div style="padding: 8px">
					<a-space direction="vertical" style="width: 100%">
						<!-- <a-input
							:value="selectedKeys[0]"
							@change="(e: any) => { setSelectedKeys(e.target!.value ? [e.target.value] : []) }"
						>
						</a-input> -->
						<ka-input
							:value="selectedKeys[0]"
							@change="
								val => {
									setSelectedKeys(val ? [val] : []);
								}
							"
							@search="val => onAntFilterSearch(column.key, val)"
							:component-type="filterCols.find(l => l.key === column.key)?.componentType"
							v-bind="filterCols.find(l => l.key === column.key)?.attrs"
							:style="{ width: filterCols.find(l => l.key === column.key)?.width }"
						>
						</ka-input>
						<a-flex justify="space-between" align="center">
							<a-button @click="onAntFilterConfirm(setSelectedKeys, selectedKeys, confirm, column)" type="primary">
								{{ props.language.toolbarFilter }}
							</a-button>
							<a-button @click="onAntFilterReset(clearFilters, confirm, column)">{{ props.language.reset }}</a-button>
						</a-flex>
					</a-space>
				</div>
			</template>
			<template #customFilterIcon="{ filtered }">
				<search-outlined :style="{ color: filtered ? props.theme : undefined }" />
			</template>
			<!-- 排序 -->
			<template #headerCell="{ column }">
				<a-flex justify="space-between" align="center">
					<span>{{ column.title }}</span>
					<a-typography-text v-show="column.sortOrder" type="secondary">{{ column.sorter.multiple }}</a-typography-text>
				</a-flex>
			</template>
		</a-table>

		<!-- 抽屉 -->
		<a-drawer
			:title="drawTitle"
			:open="isDrawOpen"
			:width="drawWidth"
			@close="onDrawCancel"
			class="ka-table-drawer"
			:closable="!loading.draw"
			:mask-closable="false"
			:force-render="true"
			:header-style="{ 'padding-top': '8px', 'padding-bottom': '8px' }"
			:body-style="{ 'padding-top': '8px', 'padding-bottom': '8px' }"
		>
			<template #extra>
				<a-space>
					<a-button type="primary" @click="onDrawSubmit" :loading="loading.draw" size="normal">
						{{ props.drawSubmitTitle || props.language.confirm }}
					</a-button>
				</a-space>
			</template>
			<a-spin :spinning="loading.draw">
				<ka-sorter
					v-if="props.toolbar.hasSort"
					ref="$sorter"
					:col-obj="sorterObj"
					v-show="tableStatus === 'Sort'"
				></ka-sorter>
				<ka-filter
					v-if="props.toolbar.hasFilter"
					v-show="tableStatus === 'Filter'"
					:columns="filterCols"
					ref="$filter"
				></ka-filter>
				<ka-editor
					v-if="props.toolbar.hasAdd || props.toolbar.hasEdit"
					ref="$form"
					v-show="['Add','Edit'].includes(tableStatus!)"
					:items-obj="editorObj"
				>
				</ka-editor>
				<a-table
					v-if="props.toolbar.hasImport"
					v-show="tableStatus === 'Import'"
					bordered
					:columns="importCols"
					:dataSource="importRecords"
					:pagination="false"
				>
					<template #footer> {{ props.language.summaryTotal }}: {{ importRecords.length }} </template>
				</a-table>
			</a-spin>
		</a-drawer>
		<a v-if="props.toolbar.hasExport" ref="$export" style="display: none"></a>
	</a-config-provider>
</template>

<script setup lang="ts">
import { Modal, TableProps, theme } from 'ant-design-vue';
import KaEditor from '../ka_editor/KaEditor.vue';
import KaFilter from '../ka_filter/KaFilter.vue';
import KaToolbar from '../ka_toolbar/KaToolbar.vue';
import KaInput from '../ka_input/KaInput.vue';
import dayjs from 'dayjs';
import {
	KaTableCol,
	KaTableDataSource,
	KaTableEventHandle,
	KaTableListCol,
	KaTableResponse,
	KaTableRowRecord,
	KaTableSearchPar,
	KaTableSearchResponse,
	KaTableStatus,
	kaTableProps,
	KaTableExportPar,
	KaTableImportCol,
	KaTableImportFileResponse,
	KaTableResponseRecord,
} from '.';
import { Ref, onBeforeMount, onMounted, reactive, ref, watch } from 'vue';
import { PaginationConfig } from 'ant-design-vue/es/pagination';
import { ColumnType, FilterValue, SorterResult } from 'ant-design-vue/es/table/interface';
import { KaFilterCol, KaFilterCondition } from '../ka_filter';
import * as lodash from 'lodash-es';
import {
	createAntCols,
	createEditorItemsObj,
	createFilterCols,
	initPorps,
	resetSorterIndex,
	setAntSorters,
	createSorterObj,
	createAntFilters,
	createAllCols,
	createExportCols,
	createImportCols,
	qsStringify,
} from './common';
import { KaEditorItem, KaEditorItemOption } from '../ka_editor';
import { KaSorterCondition } from '../ka_sorter';
import KaSorter from '../ka_sorter/KaSorter.vue';
import { SearchOutlined } from '@ant-design/icons-vue';
import axios from 'axios';
// import qs from 'qs';
import { NamePath, ValidateOptions } from 'ant-design-vue/es/form/interface';
import { FileType } from 'ant-design-vue/es/upload/interface';

// #region 扩展
/** dayjs */
dayjs.prototype.toJSON = function () {
	return this.format();
};
// #endregion 扩展

// #region data

//  #region 普通
/** props */
const props = defineProps(kaTableProps());

/** 样式 */
const { useToken } = theme;
const { token } = useToken();
const primaryColor = props.theme || token.value.colorPrimary;
const activeRowColor = props.theme ? props.theme + '55' : token.value.colorPrimaryBg;
const colsColor = props.theme ? props.theme + '33' : token.value.colorFillAlter;
const titleColor = props.theme || token.value.colorFillAlter;
const borderColor = props.theme || token.value.colorBorderSecondary;
const borderRadius = token.value.borderRadiusLG + 'px';
const tdPadding = token.value.paddingXS + 'px';

/** 加载状态 */
const loading = reactive({
	list: false,
	draw: false,
});

/** 表格操作状态 */
const tableStatus = ref<KaTableStatus>();

/** 所有字段 */
let allCols = {} as { [key: string]: KaTableCol };
// #endregion 普通

//  #region list
/** 数据源 */
const dataSource: KaTableDataSource = reactive({
	records: [],
	activeIndex: null,
	summary: null,
	get curRecord() {
		return this.activeIndex != null ? this.records[this.activeIndex!] : null;
	},
});

/** 分页信息 */
const pagination: PaginationConfig = reactive({
	current: 1,
	pageSize: props.pageSize,
	total: 0,
	showSizeChanger: false,
	showTotal: (total: number, range: number[]) => `${range[0]}-${range[1]} / ${total}`,
});

/** ant表格字段配置 */
const antCols: Ref<KaTableListCol[]> = ref([]);
// #endregion list

//  #region draw
/** 抽屉标题 */
const drawTitle = ref('');
/** 抽屉是否打开 */
const isDrawOpen = ref(false);
/** 抽屉宽度 */
const drawWidth = ref(props.drawWidth || 720);
/** 抽屉提交按钮 */
const onDrawSubmit = ref<Function | null>(null);
// #endregion draw

//  #region 排序
/** 排序dom */
const $sorter = ref<InstanceType<typeof KaSorter>>();
/** 供排序组件使用 */
// TODO:改名
const sorterObj = ref<{ [key: string]: string }>({});
/** 排序条件 */
let sorterConditions = [] as KaSorterCondition[];
/** 是否有排序 */
const isSorted = ref(false);
// #endregion 排序

//  #region 筛选
/** 筛选dom */
const $filter = ref<InstanceType<typeof KaFilter>>();
/** 筛选字段，供筛选组件使用 */
const filterCols = ref<KaFilterCol[]>([]);
/** 是否有过滤 */
const isFiltered = ref(false);
/** 目前自定义过滤条件 */
let filterConditions = [] as KaFilterCondition[];
/** 过滤是否来自ant table */
let isAntFilter = false;
// #endregion 筛选

//  #region 编辑
/** 编辑dom */
const $form = ref<InstanceType<typeof KaEditor>>();
/** 编辑项配置，供编辑组件使用 */
const editorObj = ref<{ [key: string]: KaEditorItem }>({});
/** 目前编辑值 */
let editorVals = {} as { [key: string]: any };
//  #endregion 编辑

//	#region 导出
/** 导出dom */
const $export = ref();
/** 导出字段 */
let exportCols = [] as KaTableExportPar['cols'];
// 	#endregion 导出

//	#region 导入
/** 导入字段 */
let importCols = [] as KaTableImportCol[];
/** 目前导入内容 */
const importRecords = ref<any[]>([]);
// 	#endregion 导入

//  #region watch
watch(tableStatus, newValue => {
	switch (newValue) {
		case 'Add':
			drawTitle.value = props.addTitle || props.language.toolbarAdd;
			resetDrawWidth();
			onDrawSubmit.value = addSubmit;
			isDrawOpen.value = true;
			break;
		case 'Edit':
			drawTitle.value = props.editTitle || props.language.toolbarEdit;
			resetDrawWidth();
			onDrawSubmit.value = editSubmit;
			isDrawOpen.value = true;
			break;
		case 'Sort':
			drawTitle.value = props.sortTitle || props.language.toolbarSort;
			resetDrawWidth();
			onDrawSubmit.value = sortSubmit;
			isDrawOpen.value = true;
			break;
		case 'Filter':
			drawTitle.value = props.filterTitle || props.language.toolbarFilter;
			resetDrawWidth();
			onDrawSubmit.value = filterSubmit;
			isDrawOpen.value = true;
			break;
		case 'Import':
			drawTitle.value = props.importTitle || props.language.toolbarImport;
			resetDrawWidth();
			onDrawSubmit.value = importSubmit;
			isDrawOpen.value = true;
			break;
		default:
			isDrawOpen.value = false;
			break;
	}
});

/** expose */
defineExpose({
	reRenderEditor: () => {
		$form.value?.reRender();
	},
	getEditorVal: (key?: string) => $form.value?.getEditorVal(key),
	setEditorVal: (key: string, val: any, trigChange?: boolean, trigSearch?: boolean) =>
		$form.value?.setEditorVal(key, val, trigChange, trigSearch),
	resetFields: (name?: NamePath | undefined) => $form.value?.resetFields(name),
	clearValidate: (name?: NamePath | undefined) => $form.value?.clearValidate(name),
	validate: (nameList?: string | NamePath[] | undefined, options?: ValidateOptions | undefined) =>
		$form.value?.validate(nameList, options),
	validateFields: (nameList?: string | NamePath[] | undefined, options?: ValidateOptions | undefined) =>
		$form.value?.validateFields(nameList, options),
	scrollToField: (name: NamePath, options?: {} | undefined) => $form.value?.scrollToField(name, options),
	getEditorObj: () => getEditorObj(),
	getAntCols: () => getAntCols(),
	reloadData: async () => await loadData(),
	reinit: () => {
		init();
		$filter.value?.render(filterCols.value);
	},
	setFilters: (conditions: KaFilterCondition[]) => {
		setFilters(conditions, false);
	},
	setSorters: (conditions: KaSorterCondition[]) => {
		setSorters(conditions);
	},
});
// #endregion watch

// #endregion data

// #region 方法

//  #region list
/** ant table 自定义表格事件。单击行 */
const rowEvent = (_preRecord: KaTableRowRecord, index: number | undefined) => {
	return {
		onclick: async (_event: MouseEvent) => {
			dataSource.activeIndex = index!;
			if (!(await eventHandle(props.onAfterRowClick))) return;
			// console.log('customRow onClick', index);
		},
		onDblclick: async (_event: MouseEvent) => {
			dataSource.activeIndex = index!;
			if (!(await eventHandle(props.onAfterRowDbClick))) return;
			// console.log('customRow onClick', index);
		},
	};
};
/** 行样式 */
const rowClass = (_record: any, index: number) => {
	return index === dataSource.activeIndex ? 'ka-table-selected-row' : '';
};
/** 重新计算编辑窗口宽度 */
const resetDrawWidth = () => {
	let pw = 0;
	if (lodash.isNumber(props.drawWidth)) {
		pw = props.drawWidth as number;
	} else if (lodash.isString(props.drawWidth)) {
		if (parseInt(props.drawWidth as string) === +(props.drawWidth as string)) {
			pw = parseInt(props.drawWidth as string);
		}
	} else {
		return (drawWidth.value = props.drawWidth);
	}

	if (window.innerWidth < pw) {
		return (drawWidth.value = '100%');
	}
	return (drawWidth.value = pw);
};
/** 自定义单元格显示内容，有性能问题：鼠标悬停会重渲染 */
const formatItem = (text: any, antCol: KaTableListCol, _record: KaTableRowRecord) => {
	if (text == null) return '';
	if (antCol.options) {
		if (lodash.isArray(antCol.options)) {
			return (antCol.options as KaEditorItemOption[]).find(o => o.value === text)?.label || text;
		} else if (lodash.isFunction(antCol.options)) {
			return (antCol.options as (key: string) => KaEditorItemOption[])(text).find(o => o.value === text)?.label || text;
		}
	}

	const col = lodash.get(props.columns, antCol.key as string) as KaTableCol;

	if (col.dbInfo!.dataType === 'date') {
		return dayjs(text).format(col.dbInfo!.dateFormat);
	}

	const editCol = editorObj.value[antCol.key!];
	if (editCol) {
		if (editCol.selectSplit) {
			return text.join(editCol.selectSplit);
		}
	}

	return text;
};
const getAntCols = () => {
	return antCols.value;
};
/** ant table触发表格查询 */
const onAntTableChange: TableProps['onChange'] = async (page, filters, sorters, extra) => {
	props.isDebug && console.groupCollapsed('onTableChange');
	// console.log('filters', filters);
	// console.log('sorters', sorters);
	// console.log('extra', extra);

	try {
		// 分页
		pagination.current = page.current;


		// 排序
		const _sortConditions = convertAntSortToSorterConditions(sorters, sorterConditions);
		setSorters(_sortConditions);

		//筛选
		// if (isAntFilter) {
		if(extra.action==='filter'){
			const _filterConditions = convertAntFilterToFilterConditions(filters);
			setFilters(_filterConditions, true);
		}

		// 分页
		if(extra.action==='paginate'){
			if (!(await eventHandle(props.onPrePage))) return;
		}

		await loadData();

		// 分页
		if(extra.action==='paginate'){
			if (!(await eventHandle(props.onPostPage))) return;
		}
	} catch (e: any) {
		showError(e);
	}

	props.isDebug && console.groupEnd();
};
const onToolbarRefresh = async () => {
	props.isDebug && console.groupCollapsed('onToolbarRefresh');
	loading.list = true;

	try {
		if (!(await eventHandle(props.onBeforeRefresh))) return;

		await loadData();

		if (!(await eventHandle(props.onPostRefresh))) return;
	} catch (e: any) {
		showError(e);
	} finally {
		tableStatus.value = 'List';
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};
/** 读取数据 */
const loadData = async () => {
	props.isDebug && console.groupCollapsed('loadData');

	try {
		loading.list = true;

		const _sorterConditions = sorterConditions.filter(item => item.index != null).sort((a, b) => a.index! - b.index!);

		props.isDebug && console.log('sortConditions->', JSON.stringify(_sorterConditions));
		props.isDebug && console.log('whereConditions->', JSON.stringify(filterConditions));

		const res = await axios.post<KaTableSearchResponse>(
			props.url,
			qsStringify({
				actNo: 'search',
				searchPar: JSON.stringify({
					sortConditions: _sorterConditions,
					whereConditions: [...filterConditions, ...props.frozenFilterConditions],
					pageSize: pagination.pageSize,
					pageNum: pagination.current,
				} as KaTableSearchPar),
			})
		);

		const data = res.data;

		if (!data.isSuccess) {
			throw new Error(data.message || props.language.loadError);
		} else {
			// 删除了最后一项会导致查询出空白，重新查询上一页
			if (data.total > 0 && data.records.length === 0) {
				let page = pagination.pageSize ? Math.ceil(data.total / pagination.pageSize) : 1;
				if ((pagination.current || 1) > page) {
					pagination.current = page;
					loadData();
					return;
				}
			}
			// 格式化日期和多选
			// for (const antCol of antCols.value) {
			for (const key in allCols) {
				// const col = lodash.get(props.columns, antCol.key as string) as KaTableCol;
				const col = allCols[key];
				if (col.dbInfo?.dataType === 'date') {
					for (const record of data.records) {
						lodash.update(record, col.key!, (v: string) => (v ? dayjs(v) : v));
					}
				} else if (col.editorInfo?.selectMode === 'multiple') {
					const editCol = editorObj.value[col.key!];
					if (editCol) {
						for (const record of data.records) {
							lodash.update(record, col.key!, (v: string) => (v ? v.split(editCol.selectSplit!) : v));
						}
					}
				}
			}
		}

		dataSource.records = data.records || [];
		dataSource.summary = data.summary || [];
		pagination.total = data.total || 0;
		dataSource.activeIndex = null;

		await eventHandle(props.onPostLoadData);
	} catch (error: any) {
		dataSource.records = [];
		pagination.total = 0;
		dataSource.activeIndex = null;
		throw error;
	} finally {
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};
//  #endregion list

//  #region 排序

/** 将tableChange的ant排序值转换为排序条件 */
const convertAntSortToSorterConditions = (
	antSorters: SorterResult | SorterResult[],
	curConditions: KaSorterCondition[]
) => {
	console.log('convertAntSortToSorterConditions');
	if (curConditions == null) return [];

	let maxIndex = curConditions.reduce((max, sorter) => (sorter.index! > max ? sorter.index! : max), 0);

	let sorters = (antSorters as any).length ? (antSorters as SorterResult[]) : [antSorters as SorterResult];
	for (const condition of curConditions) {
		const sorter = sorters.find(s => s.columnKey === condition.key);
		if (sorter == null || sorter.order == null) {
			condition.index = null;
			condition.order = null;
		} else {
			condition.order = sorter.order!;
			if (condition.index == null) {
				condition.index = ++maxIndex;
			}
		}
	}

	resetSorterIndex(curConditions);

	return curConditions;
};
/** 点击工具栏排序 */
const onToolbarSort = async () => {
	props.isDebug && console.groupCollapsed('onToolbarSort');
	loading.list = true;
	loading.draw = true;

	try {
		if (!(await eventHandle(props.onBeforeSort))) return;

		$sorter.value?.setSorter(sorterConditions);

		tableStatus.value = 'Sort';

		if (!(await eventHandle(props.onAfterSort))) return;
	} catch (e: any) {
		showError(e);
	} finally {
		loading.list = false;
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
/** 提交排序 */
const sortSubmit = async () => {
	props.isDebug && console.log('sortSubmit');
	loading.draw = true;

	try {
		if (!(await eventHandle(props.onPreSort))) return;

		const conditions = $sorter.value?.getSorter();
		setSorters(conditions!);
		await loadData();

		if (!(await eventHandle(props.onPostSort))) return;

		tableStatus.value = 'List';
	} catch (error: any) {
		showError(error);
	} finally {
		loading.draw = false;
	}
};
/** 设置排序值 */
const setSorters = (conditions: KaSorterCondition[]) => {
	const allConditions = [];
	for (const key in sorterObj.value) {
		const condition = conditions.find(item => item.key === key);
		if (condition) {
			allConditions.push({
				key: key,
				index: condition.index,
				order: condition.order,
			});
		} else {
			allConditions.push({
				key: key,
				order: null,
				index: null,
			});
		}
	}
	if (props.toolbar.hasSort) {
		setAntSorters(antCols.value, allConditions);
	} else {
		setAntSorters(antCols.value, []);
	}
	sorterConditions = allConditions;
	isSorted.value = conditions.some(col => col.order);
};
//  #endregion 排序

//  #region 筛选
/** 确认筛选 */
const onAntFilterConfirm = (_setSelectedKeys: any, selectedKeys: any[], confirm: Function, column: ColumnType) => {
	// console.log(setSelectedKeys);
	// console.log(selectedKeys);
	// confirm();
	// console.log(confirm);
	// console.log(clearFilters);
	// setSelectedKeys([$selectInput.value.input.input.value]);
	// const val = $selectInput.value.input.input.value;
	// column.filteredValue = ['x123'];
	// setSelectedKeys(['x123']);
	// console.log(selectedKeys);

	isAntFilter = true;
	column.filteredValue = selectedKeys;
	confirm();

	// console.log(inputDom.value.input.input);
	// if(selectedKeys)
};
/** 重置筛选 */
const onAntFilterReset = (clearFilters: Function, confirm: Function, column: ColumnType) => {
	isAntFilter = true;
	clearFilters();
	column.filteredValue = [];
	confirm();
};
/** 将tableChange的ant筛选值转换为筛选条件 */
const convertAntFilterToFilterConditions = (filters: Record<string, FilterValue | null>) => {
	const conditions = [] as KaFilterCondition[];
	for (const key in filters) {
		const filter = filters[key];
		if (filter && filter.length) {
			const col = filterCols.value.find(col => col.key === key);
			let opt: KaFilterCondition['opt'] = 'eq';
			if (col?.componentType === 'input') {
				opt = 'like';
			}

			conditions.push({
				key: key,
				opt: opt,
				bool: 'and',
				val: filter!.length === 1 ? filter![0] : filter,
			});
		}
	}
	return conditions;
};
const onToolbarFilter = async () => {
	props.isDebug && console.groupCollapsed('onToolbarFilter');
	loading.list = true;
	loading.draw = true;

	try {
		if (!(await eventHandle(props.onBeforeFilter))) return;

		$filter.value?.setConditions(filterConditions);

		tableStatus.value = 'Filter';

		if (!(await eventHandle(props.onAfterFilter))) return;
	} catch (e) {
		showError(e);
	} finally {
		loading.draw = false;
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};
const filterSubmit = async () => {
	props.isDebug && console.log('filterSubmit');
	loading.draw = true;
	isAntFilter = false;

	try {
		if (!(await eventHandle(props.onPreFilter))) return;

		setFilters($filter.value?.getConditions() || [], false);
		await loadData();

		if (!(await eventHandle(props.onPostFilter))) return;

		tableStatus.value = 'List';
	} catch (error: any) {
		showError(error);
	} finally {
		loading.draw = false;
	}
};
/** table表头上的筛选关键字查询 */
const onAntFilterSearch = async (key: string, value: any) => {
	const filterCol = filterCols.value.find(col => col.key === key);
	if (filterCol) {
		if (filterCol.options && lodash.isFunction(filterCol.options)) {
			filterCol.attrs!.options = await (filterCol.options as (key: string) => Promise<KaEditorItemOption[]>)(value);
		}
	}
};
/** 清空table表头上的筛选 */
const clearAntFilters = () => {
	for (const col of antCols.value) {
		if (col.customFilterDropdown) {
			col.filteredValue = [];
		}
	}
};
/** 设置筛选值 */
const setFilters = (conditions: KaFilterCondition[], isFromAnt: boolean = true) => {
	!isFromAnt && clearAntFilters();
	$filter.value?.setConditions(conditions);
	isFiltered.value = !!conditions.length;
	filterConditions = conditions;
};
//  #endregion 筛选

//  #region 编辑
/** 设置编辑项目 */
const setEditorItems = () => {
	const curRecord = dataSource.curRecord;
	for (const key in editorObj.value) {
		const val = lodash.cloneDeep(lodash.get(curRecord, key));
		$form.value?.setEditorVal(key, val, false, true);
	}
};
const onToolbarEdit = async () => {
	props.isDebug && console.groupCollapsed('onToolbarEdit');
	loading.list = true;
	loading.draw = true;

	try {
		if (dataSource.activeIndex == null) {
			throw props.language.selectRow;
		}

		if (!(await eventHandle(props.onBeforeEdit))) return;

		setEditorItems();
		tableStatus.value = 'Edit';

		if (!(await eventHandle(props.onAfterEdit))) return;
	} catch (e: any) {
		showError(e);
	} finally {
		$form.value?.clearValidate();
		loading.list = false;
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
const onToolbarAdd = async () => {
	props.isDebug && console.groupCollapsed('onToolbarAdd');
	loading.list = true;
	loading.draw = true;

	try {
		if (!(await eventHandle(props.onBeforeAdd))) return;

		$form.value?.clearEditor();
		tableStatus.value = 'Add';

		if (!(await eventHandle(props.onAfterAdd))) return;
	} catch (e: any) {
		showError(e);
	} finally {
		loading.list = false;
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
const addSubmit = async () => {
	props.isDebug && console.groupCollapsed('addSubmit');
	loading.draw = true;
	try {
		await $form.value?.validate().catch((e: any) => {
			if (e.errorFields) {
				if (e.errorFields.length) {
					throw e.errorFields[0].errors[0];
				}
			}
		});
		editorVals = $form.value?.getEditorVal();

		if (!(await eventHandle(props.onPreAddOrEdit))) return;
		if (!(await eventHandle(props.onPreAdd))) return;

		const newData = await insertData();

		if (!(await eventHandle(props.onPostAddOrEdit, newData))) return;
		if (!(await eventHandle(props.onPostAdd, newData))) return;

		showAlert(props.language.addSuccess);
		tableStatus.value = 'List';

		await loadData();
	} catch (e) {
		showError(e);
	} finally {
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
const editSubmit = async () => {
	props.isDebug && console.groupCollapsed('editSubmit');
	loading.draw = true;
	try {
		await $form.value?.validate().catch((e: any) => {
			if (e.errorFields) {
				if (e.errorFields.length) {
					throw e.errorFields[0].errors[0];
				}
			}
		});
		editorVals = $form.value?.getEditorVal();

		if (!(await eventHandle(props.onPreAddOrEdit))) return;
		if (!(await eventHandle(props.onPreEdit))) return;

		const newData = await updateData();

		if (!(await eventHandle(props.onPostAddOrEdit, newData))) return;
		if (!(await eventHandle(props.onPostEdit, newData))) return;

		showAlert(props.language.editSuccess);
		tableStatus.value = 'List';

		await loadData();
	} catch (e) {
		showError(e);
	} finally {
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
/** 新增数据 */
const insertData = async () => {
	props.isDebug && console.log('insertData');

	const record = JSON.stringify(getEditorValObj(false, true, true));
	if (record === '{}') {
		throw props.language.noChange;
	}

	const res = await axios.post<KaTableResponseRecord>(
		props.url,
		qsStringify({
			actNo: 'insert',
			record: record,
		})
	);
	if (!res.data.isSuccess) {
		throw res.data.message || props.language.addError;
	}
	return res.data.record;
};
/** 更新数据 */
const updateData = async () => {
	props.isDebug && console.log('updateData');

	const newRecord = JSON.stringify(getEditorValObj(true, true, true));
	if (newRecord === '{}') {
		throw props.language.noChange;
	}

	const res = await axios.post<KaTableResponseRecord>(
		props.url,
		qsStringify({
			actNo: 'update',
			newRecord,
			oldRecord: JSON.stringify(getCurRecordJson()),
		})
	);
	if (!res.data.isSuccess) {
		throw res.data.message || props.language.editError;
	}
	return res.data.record;
};
const getEditorObj = () => {
	return editorObj.value;
};
//  #endregion 编辑

//	#region 删除
const onToolbarRemove = async () => {
	props.isDebug && console.groupCollapsed('onToolbarRemove');
	loading.list = true;

	try {
		if (dataSource.activeIndex == null) {
			throw props.language.selectRow;
		}

		if (!(await eventHandle(props.onPreRemove))) return;

		tableStatus.value = 'Remove';
		await removeData();

		if (!(await eventHandle(props.onPostRemove))) return;

		showAlert(props.language.removeSuccess);
		tableStatus.value = 'List';
		await loadData();
	} catch (e: any) {
		showError(e);
	} finally {
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};

const removeData = async () => {
	props.isDebug && console.log('removeData');

	const res = await axios.post<KaTableResponse>(
		props.url,
		qsStringify({
			actNo: 'remove',
			record: JSON.stringify(getCurRecordJson()),
		})
	);
	if (!res.data.isSuccess) {
		throw res.data.message || props.language.removeError;
	}
};

//	#endregion 删除

//  #region 导出
const onToolbarExport = async (isAll: boolean) => {
	props.isDebug && console.groupCollapsed('onToolbarExport');
	loading.list = true;

	try {
		if (!(await eventHandle(props.onBeforeExport))) return;

		await exportData(isAll);

		if (!(await eventHandle(props.onAfterExport))) return;
	} catch (e) {
		showError(e);
	} finally {
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};
const exportData = async (isAll: boolean) => {
	await eventHandle(props.onPreExport);

	const _sorterConditions = sorterConditions.filter(item => item.order != null);
	const fileName = `${props.tableTitle}(${dayjs().format('YYYYMMDDHHmmss')})`;

	const res = await axios.post<KaTableResponse>(
		props.url,
		qsStringify({
			actNo: 'export',
			exportPar: JSON.stringify({
				sortConditions: _sorterConditions,
				whereConditions: [...filterConditions, ...props.frozenFilterConditions],
				pageNum: pagination.current,
				pageSize: isAll ? 0 : pagination.pageSize,
				cols: exportCols,
				fileName,
			} as KaTableExportPar),
		}),
		{
			responseType: 'blob',
		}
	);

	if (res.headers['content-type']?.toString().includes('application/json')) {
		throw JSON.parse(await (res.data as any).text()).Message || props.language.exportError;
	}

	const file = new Blob([res.data as any], {
		type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
	});
	$export.value.href = URL.createObjectURL(file);
	$export.value.download = props.exportFileName ? props.exportFileName() : fileName;
	//$export.setAttribute('download', `考勤-${dayjs().format('MMDDHHmmss')}.xlsx`);
	$export.value.click();

	await eventHandle(props.onPostExport);
};
//  #endregion 导出

//	#region 导入
const uploadFile = async (file: FileType) => {
	props.isDebug && console.groupCollapsed('uploadFile');
	loading.list = true;

	try {
		if (file == null) {
			return;
		}

		const formData = new FormData();
		formData.append('file', file);
		formData.append('cols', JSON.stringify(importCols.map(c => ({ key: c.key, title: c.title }))));
		formData.append('actNo', 'import_file');

		const res = await axios.post<KaTableImportFileResponse>(props.url, formData);

		const data = res.data;

		if (!data.isSuccess) {
			throw new Error(data.message || props.language.importFileError);
		} else {
			importRecords.value = data.records;
			tableStatus.value = 'Import';
		}
	} catch (e: any) {
		showError(e);
	} finally {
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};
const importSubmit = async () => {
	props.isDebug && console.groupCollapsed('importSubmit');
	loading.draw = true;
	try {
		if (!(await eventHandle(props.onPreImport, importRecords.value))) return;

		await importData();

		if (!(await eventHandle(props.onPostImport, importRecords.value))) return;

		showAlert(props.language.importSuccess);
		tableStatus.value = 'List';
		await loadData();
	} catch (e) {
		showError(e);
	} finally {
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
const importData = async () => {
	props.isDebug && console.log('importData');

	const res = await axios.post<KaTableResponse>(
		props.url,
		qsStringify({
			actNo: 'import',
			records: JSON.stringify(importRecords.value),
		})
	);
	if (!res.data.isSuccess) {
		throw res.data.message || props.language.importError;
	}
};
const downloadTemplate = async () => {
	const res = await axios.post<KaTableResponse>(
		props.url,
		qsStringify({
			actNo: 'download_template',
			cols: JSON.stringify(importCols.map(c => ({ key: c.key, title: c.title }))),
		}),
		{
			responseType: 'blob',
		}
	);

	if (res.headers['content-type']?.toString().includes('application/json')) {
		throw JSON.parse(await (res.data as any).text()).Message || props.language.templateError;
	}

	const file = new Blob([res.data as any], {
		type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
	});
	$export.value.href = URL.createObjectURL(file);
	$export.value.download = 'template.xlsx';
	//$export.setAttribute('download', `考勤-${dayjs().format('MMDDHHmmss')}.xlsx`);
	$export.value.click();
};
//	#endregion 导入

//  #region 初始化
const init = () => {
	props.isDebug && console.log('init');
	initPorps(props);

	const _editorObj = createEditorItemsObj(props.columns!);
	const _filterCols = createFilterCols(props.columns!, _editorObj);
	const _sorterObj = createSorterObj(props.columns!);
	const _antCols = createAntCols(props.columns!);
	exportCols = createExportCols(props.columns!);
	importCols = createImportCols(props.columns!);
	allCols = createAllCols(props.columns!);
	// console.log(_filterCols);

	// 标题上显示筛选图标
	if (props.toolbar.hasFilter) {
		createAntFilters(_antCols, _filterCols);
	}

	editorObj.value = _editorObj;
	filterCols.value = _filterCols;
	sorterObj.value = _sorterObj;
	antCols.value = _antCols;

	setSorters(props.initSorterConditions);
	setFilters(props.initFilterConditions);

	if (!importCols.length) {
		props.toolbar.hasImport = false;
	}
};
//  #endregion 初始化

//  #region 其它

/** toolbar事件传参 */
const eventHandle = async (event?: KaTableEventHandle, data?: any) => {
	if (event) {
		const handleResult = await event(data || { ...dataSource, editorRecord: getEditorValObj() });
		if (!handleResult.isSuccess) {
			showError(handleResult.message || props.language.refreshError);
			return false;
		}

		if (handleResult.message) {
			let isConfirm = true;
			if (handleResult.alertType === 'alert') {
				await new Promise<boolean>((resolve, _reject) => {
					showAlert(handleResult.message!, resolve);
				});
			} else {
				isConfirm = await new Promise<boolean>((resolve, _reject) => {
					showConfirm(resolve, handleResult.message!);
				});
			}

			if (!isConfirm) {
				return false;
			}
		}
	}
	return true;
};
/** 将编辑值转换为object
 *
 * @param isDiff 是否是差异比较
 * @param isJson 是否转换json格式
 * @param isPost 是否过滤post参数
 */
const getEditorValObj = (isDiff: boolean = false, isJson: boolean = false, isPost: boolean = false) => {
	const obj = {};
	const oldRecord = dataSource.curRecord;
	try {
		for (const key in editorVals) {
			let newVal = editorVals[key];
			let oldVal = isDiff ? lodash.get(oldRecord, key) : newVal + 'x';
			const col = editorObj.value[key];
			if (isJson) {
				if (col.componentType === 'select' && col.selectSplit) {
					if (newVal != null) {
						newVal = newVal.length ? newVal.join(col.selectSplit) : null;
					}
					if (isDiff && oldVal != null) {
						oldVal = oldVal.length ? oldVal.join(col.selectSplit) : null;
					}
				}
			}
			// console.log(newVal + '' , oldVal + '',isPost,col.isPost);
			if (newVal + '' !== oldVal + '') {
				if ((isPost && col.isPost) || !isPost) {
					lodash.set(obj, key, newVal);
				}
			}
		}
	} catch (e) {
		console.error(e);
		throw e;
	}

	return obj;
};
/** 将编辑值转为json */
// const _getEditorJson = () => {
// 	try {
// 		const data = lodash.cloneDeep(editorVals);
// 		for (const key in data) {
// 			const col = editorObj.value[key];
// 			if (col.componentType === 'select' && col.selectSplit) {
// 				data[key] = data[key].join(col.selectSplit);
// 			}
// 		}
// 		return data;
// 	} catch (e) {
// 		console.error(e);
// 	}
// };
/** 转换当前记录为已序列化的对象 */
const getCurRecordJson = () => {
	try {
		const curRecord = dataSource.curRecord;
		const result = lodash.cloneDeep(curRecord)!;
		for (const key in allCols) {
			const col = allCols[key];
			const editCol = editorObj.value[key];
			if (col.editorInfo?.isPost !== false) {
				let val = lodash.get(curRecord, key);
				if (editCol?.componentType === 'select' && editCol?.selectSplit && val != null) {
					val = val.length ? val.join(editCol.selectSplit) : null;
				}
				lodash.set(result, key, val);
			}
		}
		return result;
	} catch (e) {
		console.error(e);
		throw e;
	}
};

/** 确认框 */
const showConfirm = (resolve: (value: boolean | PromiseLike<boolean>) => void, message: string) => {
	Modal.confirm({
		title: message,
		onOk() {
			resolve(true);
		},
		onCancel() {
			resolve(false);
		},
	});
};
/** 错误框 */
const showError = (message: any) => {
	Modal.error({ title: lodash.isString(message) ? message : message.message });
};
/** 提示框 */
const showAlert = (message: any, resolve?: (value: boolean | PromiseLike<boolean>) => void) => {
	Modal.info({
		title: lodash.isString(message) ? message : message.message,
		onOk() {
			resolve && resolve(true);
		},
	});
};
/** 关闭抽屉 */
const onDrawCancel = () => {
	tableStatus.value = null;
};
//  #endregion 其它

// #endregion 方法

// #region 生命周期

onBeforeMount(() => {
	init();
});
onMounted(async () => {
	if (props.autoLoad) {
		try {
			await loadData();
		} catch (e: any) {
			showError(e);
		}
	}
});
// #endregion 生命周期
</script>

<style scoped>
.ka-table :deep(.ant-table-title) {
	padding: 0.4rem;
	background-color: v-bind(titleColor);
}

.ka-table-title {
	padding: 0;
}

.ka-table-title > h4 {
	margin: 0;
}

.ka-table :deep(.ka-table-selected-row) td {
	background-color: v-bind(activeRowColor) !important;
}

.ka-table :deep(.ant-table-column-sort) {
	background-color: #fff;
}

.ka-table :deep(.ant-table-summary) {
	background-color: v-bind(colsColor) !important;
}

.ka-table :deep(.ant-table-pagination) {
	margin: 0;
	padding: v-bind(tdPadding);
	border: 1px solid v-bind(borderColor);
	border-top: 0;
	border-radius: 0 0 v-bind(borderRadius) v-bind(borderRadius);
	background-color: v-bind(colsColor);
}

:global(.ka-table-drawer .edit-item-inline-block) {
	display: flex;
	flex-direction: row;
	gap: 1rem;
}

:global(.ka-table-drawer .ant-form-item-label) {
	padding-bottom: 0.2rem;
}
:global(.ka-table-drawer .ant-form-item) {
	margin-bottom: 1rem;
}
</style>
