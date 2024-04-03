<template>
	<a-config-provider
		:component-size="props.size"
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
			<!-- 自定义单元格 -->
			<template #bodyCell="{ text, column, index, record }">
				<slot name="bodyItem" :column="column" :index="index" :text="text" :record="record">
					{{ formatItem(text, column, record) }}</slot
				>
			</template>

			<template #title>
				<!-- 标题栏 -->
				<a-flex class="ka-table-title" justify="space-between" align="center">
					<!-- 标题 -->
					<a-typography-title :level="4">{{ props.tableTitle }}</a-typography-title>
					<!-- 工具栏 -->
					<div>
						<katable-toolbar
							:refresh="{
								isShow: props.toolbar.hasRefresh,
								title: props.language.toolbarRefresh,
								onClick: onToolbarRefresh,
							}"
							:filter="{
								isShow: props.toolbar.hasFilter,
								title: props.language.toolbarFilter,
								isActive: whereConditions.length > 0,
								onClick: onToolbarFilter,
							}"
							:export="{
								isShow: props.toolbar.hasExport,
								title: props.language.toolbarExport,
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
								title: props.language.toolbarSort,
								onClick: onToolbarSort,
							}"
							:add="{ isShow: props.toolbar.hasAdd, title: props.language.toolbarAdd, onClick: onToolbarAdd }"
							:import="{ isShow: props.toolbar.hasImport, title: props.language.toolbarImport }"
							:edit="{
								isActive: dataSource.activeIndex != null,
								isShow: props.toolbar.hasEdit,
								title: props.language.toolbarEdit,
								onClick: onToolbarEdit,
							}"
							:remove="{ isShow: props.toolbar.hasRemove, title: props.language.toolbarRemove }"
							:activate-color="primaryColor"
						>
							<template #toolbar><slot name="toolbar" :data-source="dataSource"></slot></template>
						</katable-toolbar>
					</div>
				</a-flex>
			</template>
			<!-- 筛选框 -->
			<template #customFilterDropdown="{ setSelectedKeys, selectedKeys, confirm, clearFilters, column }">
				<div style="padding: 8px">
					<a-space direction="vertical">
						<a-input
							:value="selectedKeys[0]"
							@change="(e: any) => { setSelectedKeys(e.target!.value ? [e.target.value] : []) }"
						>
						</a-input>
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
				<ka-drag-list v-if="props.toolbar.hasSort" v-show="tableStatus === 'Sort'" v-model="sorterList">
					<template #renderItem="{ item }">
						<span :style="{ color: primaryColor }" style="width: 1rem">
							<sort-ascending-outlined v-show="item.order === 'ascend'" />
							<sort-descending-outlined v-show="item.order === 'descend'" />
						</span>

						{{ item.title }}
					</template>
					<template #renderAction="{ item }">
						<a-radio-group
							v-model:value="item.order"
							@change="item.disabled = item.order === null"
							button-style="solid"
						>
							<a-radio-button value="ascend"><sort-ascending-outlined /></a-radio-button>
							<a-radio-button value="descend"><sort-descending-outlined /></a-radio-button>
							<a-radio-button :value="null"><minus-outlined /></a-radio-button>
						</a-radio-group>
					</template>
				</ka-drag-list>
				<ka-table-filter
					v-if="props.toolbar.hasFilter"
					v-show="tableStatus === 'Filter'"
					:columns="filterColumns"
					v-model="filterList"
					ref="$filter"
				></ka-table-filter>
				<ka-table-editor
					ref="$form"
					v-if="props.toolbar.hasAdd || props.toolbar.hasEdit"
					v-show="['Add','Edit'].includes(tableStatus!)"
					v-model="dataSource.editRecord"
					:columns="props.columns"
					:init-handle="initHandle"
				>
				</ka-table-editor>
			</a-spin>
		</a-drawer>
		<a ref="$export" style="">xxx</a>
	</a-config-provider>
</template>

<script setup lang="ts">
import { FormInstance, Modal, TableColumnProps, TableProps, theme } from 'ant-design-vue';
import { SearchOutlined, SortAscendingOutlined, SortDescendingOutlined, MinusOutlined } from '@ant-design/icons-vue';
import KatableToolbar from '../katable_toolbar/KaTableToolbar.vue';
import KaDragList from '../kadrag_list/KaDragList.vue';
import KaTableFilter from '../katable_filter/KaTableFilter.vue';
import {
	KaTableCol,
	KaTableCols,
	KaTableDataSource,
	KaTableEventHandle,
	KaTableExportPar,
	KaTableOptionItem,
	KaTablePage,
	KaTableResponse,
	KaTableRowRecord,
	KaTableSearchPar,
	KaTableSearchResponse,
	KaTableSortCondition,
	KaTableSortOrder,
	KaTableStatus,
	KaTableWhereCondition,
	kaTableProps,
} from '.';
import { Ref, computed, onBeforeMount, onMounted, reactive, ref, watch } from 'vue';
import { PaginationConfig } from 'ant-design-vue/es/pagination';
import lodash from 'lodash';
import dayjs from 'dayjs';
import { ColumnType, FilterValue, SortOrder, SorterResult } from 'ant-design-vue/es/table/interface';
import axios from 'axios';
import qs from 'qs';
import { KaTableFilterCol, KaTableFilterCondition } from '../katable_filter';
import KaTableEditor from '../katable_editor/KaTableEditor.vue';
import { NamePath } from 'ant-design-vue/es/form/interface';

/** dayjs */
dayjs.prototype.toJSON = function () {
	return this.format();
};

// #region data
/** props */
const props = defineProps(kaTableProps());

/** 颜色 */
const { useToken } = theme;
const { token } = useToken();
const primaryColor = props.theme || token.value.colorPrimary;
const activeRowColor = props.theme ? props.theme + '55' : token.value.colorPrimaryBg;
const colsColor = props.theme ? props.theme + '33' : token.value.colorFillAlter;
const titleColor = props.theme || token.value.colorFillAlter;

/** form ref */
const $form = ref<InstanceType<typeof KaTableEditor>>(null as unknown as InstanceType<typeof KaTableEditor>);

defineExpose({
	resetFields: (nameList?: NamePath[]) => {
		console.log(nameList);
		$form.value.resetFields(nameList);
	},
	clearValidate: (nameList?: NamePath[]) => {
		$form.value.clearValidate(nameList);
	},
	validate: (
		nameList?: NamePath[]
	): Promise<{
		[key: string]: any;
	}> => $form.value.validate(nameList),
	validateFields: (
		nameList?: NamePath[]
	): Promise<{
		[key: string]: any;
	}> => $form.value.validateFields(nameList),
	scrollToField: (name: NamePath, options: [ScrollOptions]) => {
		$form.value.scrollToField(name, options);
	},
});

/** 下载dom */
const $export = ref();

/** 加载状态 */
const loading = reactive({
	list: false,
	draw: false,
});
/** 操作状态 */
const tableStatus = ref<KaTableStatus>();

/** 筛选条件 */
let whereConditions: KaTableWhereCondition[] = [];

/** 数据源 */
const dataSource: KaTableDataSource = reactive({
	records: [],
	activeIndex: null,
	editRecord: {},
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
	showTotal: (total: number, range: number[]) => `${range[0]}-${range[1]} of ${total} items`,
});
/** ant表格字段配置 */
const antCols: Ref<TableColumnProps[]> = ref([]);

/** 抽屉标题 */
const drawTitle = ref('');
/** 抽屉是否打开 */
const isDrawOpen = ref(false);
/** 抽屉宽度 */
const drawWidth = ref(props.drawWidth || 720);
/** 供排序组件使用 */
const sorterList = ref<{ col: string; order: SortOrder; title: string; disabled: boolean }[]>([]);
/** 筛选组件 */
const $filter = ref<InstanceType<typeof KaTableFilter>>();
/** 供筛选组件使用 */
const filterList = ref<KaTableFilterCondition[]>([]);
/** 供筛选组件使用 */
const filterColumns = ref<KaTableFilterCol[]>([]);
/** 抽屉提交按钮 */
const onDrawSubmit = ref<Function | null>(null);
/** 所有列 */
const allCols = [] as KaTableCol[];
/** 新增修改的列 */
const insertCols= [] as KaTableCol[];
/** 多选列 */
const multipleCols = [] as string[];
/** 日期列 */
const dateCols = [] as string[];
/** 首次填入编辑项目 */
const initHandle = ref<'only_search'|'with_search'|''>('');
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
		default:
			isDrawOpen.value = false;
			break;
	}
});

// #endregion

// #region 方法
/** 自定义行事件 */
const rowEvent = (preRecord: KaTableRowRecord, index: number | undefined) => {
	return {
		onclick: (_event: MouseEvent) => {
			if (dataSource.activeIndex !== null && dataSource.activeIndex >= 0) {
				dataSource.records[dataSource.activeIndex].isActive = false;
			}
			dataSource.activeIndex = index!;
			preRecord.isActive = !preRecord.isActive;
			console.log('customRow onClick', index);
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
/** 自定义单元格显示内容 */
const formatItem = (text: any, antCol: ColumnType, record: KaTableRowRecord) => {
	const col = lodash.get(props.columns, antCol.key as string) as KaTableCol;

	if (col.listInfo.options) {
		if (lodash.isArray(col.listInfo.options)) {
			return (col.listInfo.options as KaTableOptionItem[]).find(o => o.value === text)?.label || text;
		} else if (lodash.isFunction(col.listInfo.options)) {
			return col.listInfo.options(record);
		}
	}

	if (col.dbInfo!.dataType === 'date') {
		return dayjs(text).format(col.dbInfo!.dateFormat);
	} else if (col.editorInfo?.selectMode === 'multiple') {
		return text ? text.join(col.editorInfo.selectSplit) : text;
	}
	return text;
};
/** 是否有排序 */
const isSorted = computed(() => antCols.value.some(col => col.sortOrder));
/** 设置排序值 */
const setSorterOrders = (sorters: KaTableSortCondition[]) => {
	let index = 1;
	for (const antCol of antCols.value) {
		if (!antCol.sorter) continue;
		const sorter = sorters.find(s => s.col === antCol.key);
		// const col = lodash.get(props.columns, antCol.key as string) as KaTableCol;
		// console.log(JSON.stringify(col.sortInfo),JSON.stringify(sorter))
		// col.sortInfo!.order = sorter ? sorter.order : null;
		// col.sortInfo!.index = sorter ? index : null;
		lodash.set(props.columns, antCol.key + '.sortInfo.order', sorter ? sorter.order : null);
		lodash.set(props.columns, antCol.key + '.sortInfo.index', sorter ? index : null);
		if (sorter) {
			index++;
		}
	}
};
/** 设置ant排序 */
const setAntSortOrders = (sorters: KaTableSortCondition[]) => {
	const allKeys = new Set(antCols.value.map(col => col.key));
	let index = 1;
	for (const sorter of sorters) {
		setAntSortOrder({ ...sorter, index }, null);
		allKeys.delete(sorter.col);
		index++;
	}
	for (const key of allKeys.keys()) {
		setAntSortOrder({ col: key as string, order: null, index: null }, null);
	}
};
const setAntSortOrder = (sorter: KaTableSortOrder, antCol: TableColumnProps | null) => {
	const col = lodash.get(props.columns, sorter.col as string) as KaTableCol;
	if (antCol == null) {
		antCol = antCols.value.find(col => col.key === sorter.col) as TableColumnProps;
	}
	if (col.sortInfo) {
		if (sorter.index != null) {
			// antCol.defaultSortOrder = col.sortInfo.order;
			antCol.sortOrder = sorter.order;
			antCol.sorter = { multiple: sorter.index };
		} else {
			antCol.sortOrder = null;
			antCol.sorter = { multiple: 0 };
		}
	} else {
		antCol.sortOrder = null;
		antCol.sorter = false;
	}
};
/** 将tableChange的ant排序值转换为排序条件 */
const convertAntSortToSortConditions = (antSorters: SorterResult | SorterResult[]) => {
	let sorters = (antSorters as any).length ? (antSorters as SorterResult[]) : [antSorters as SorterResult];
	const conditions: KaTableSortCondition[] = [];

	for (const sorter of sorters) {
		if (sorter.columnKey) {
			conditions.push({ col: sorter.columnKey as string, order: sorter.order! });
		}
	}
	return conditions;
};
/** 将ant columns的排序转换为排序条件 */
const createSortConditions = () => {
	const cols = antCols.value.filter(col => col.sortOrder != null).sort(col => (col.sorter as any).multiple);
	const conditions: KaTableSortCondition[] = [];
	for (const col of cols) {
		conditions.push({ col: col.key as string, order: col.sortOrder! });
	}
	return conditions;
};

/** 确认筛选 */
const onAntFilterConfirm = (setSelectedKeys: any, selectedKeys: any[], confirm: Function, column: ColumnType) => {
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

	column.filteredValue = selectedKeys;
	confirm();

	// console.log(inputDom.value.input.input);
	// if(selectedKeys)
};
/** 重置筛选 */
const onAntFilterReset = (clearFilters: Function, confirm: Function, column: ColumnType) => {
	clearFilters();
	column.filteredValue = [];
	confirm();
};
/** 确认标题栏的筛选之后 */
const onChangeAntFilters = (filters: Record<string, FilterValue | null>) => {
	whereConditions = [];
	for (const key in filters) {
		const filter = filters[key];
		if (filter && filter.length) {
			const col = lodash.get(props.columns, key) as KaTableCol;
			let opt: KaTableWhereCondition['opt'] = 'eq';
			if (col.dbInfo!.dataType === 'string') {
				opt = 'like';
			} else if (filter!.length > 1) {
				opt = 'in';
			}

			whereConditions.push({
				col: key,
				opt: opt,
				bool: 'and',
				val: filter!.length === 1 ? filter![0] : filter,
			});
		}
	}
};
/** 清空编辑项目 */
const clearEditorItems = () => {
	for (const col of allCols) {
		lodash.set(dataSource.editRecord, col.key!, ['multiple', 'tags'].includes(col.editorInfo?.selectMode!) ? [] : null);
	}
};
/** 设置编辑项目 */
const setEditorItems = () => {
	// console.log(dataSource.curRecord)
	// dataSource.editRecord = lodash.cloneDeep(dataSource.curRecord)!;
	const curRecord = dataSource.curRecord;
	for (const col of allCols) {
		lodash.set(dataSource.editRecord, col.key!, lodash.get(curRecord, col.key!)!);
	}
};

/** ant触发表格查询 */
const onAntTableChange: TableProps['onChange'] = (page: KaTablePage, filters, sorter, _extra) => {
	props.isDebug && console.groupCollapsed('onTableChange');
	// console.log('filters', filters);
	// console.log('sorters', sorter);
	// console.log('extra', extra);
	// loading.list = true;

	try {
		// 分页
		pagination.current = page.current;

		// 排序
		const sortConditions = convertAntSortToSortConditions(sorter);
		setSorterOrders(sortConditions);
		setAntSortOrders(sortConditions);

		//筛选
		onChangeAntFilters(filters);

		//test
		loadData();
	} catch (e: any) {
		showError(e);
	}

	props.isDebug && console.groupEnd();
};
/** 处理columns */
const initCols = (colObj: KaTableCol | { [key: string]: KaTableCol }, path: string[]) => {
	// console.log(colObj, Object.hasOwn(colObj, 'title'), lodash.isString(colObj.title));
	if (Object.hasOwn(colObj, 'title')) {
		if (lodash.isString(colObj.title)) {
			// const antCol = initCol(colObj as KaTableCol, [...path]);
			antCols.value.push(initCol(colObj as KaTableCol, [...path]));
			return;
		}
	}
	if (lodash.isString(colObj)) throw new Error(`column配置错误:(${colObj})`);
	for (const key in colObj) {
		const subObj = (colObj as { [key: string]: KaTableCol })[key];
		initCols(subObj, [...path, key]);
	}
};
/** 处理col */
const initCol = (col: KaTableCol, path: string[]): TableColumnProps => {
	const key = path.join('.');
	col.key = key;
	col._katableIsCol = true;
	allCols.push(col);

	// dbInfo
	if (!col.dbInfo) {
		col.dbInfo = {
			dataType: 'string',
		};
	} else {
		if (col.dbInfo.dataType == null) {
			col.dbInfo.dataType = 'string';
		} else if (col.dbInfo.dataType === 'date') {
			if (col.dbInfo.dateFormat === undefined) {
				col.dbInfo.dateFormat = 'YYYY/MM/DD';
			}
			dateCols.push(key);
		}
	}
	//listInfo
	if (col.listInfo) {
		if (col.listInfo.index === undefined) {
			throw `列(${col.key})未设置(listInfo.index)`;
		}
		if (!col.listInfo.title) {
			col.listInfo.title = col.title?.toString() || lodash.last(path);
		}
		if (col.listInfo.width === undefined) {
			col.listInfo.width = 100;
		}
	}

	// 筛选
	// if (props.initFilterConditions) {
	// 	whereConditions = props.initFilterConditions;
	// }
	if (whereConditions.length > 0) {
		if (whereConditions.some(c => c.col === col.key)) {
			if (!col.filterInfo) {
				col.filterInfo = {};
			}
			col.filterInfo.isFilter = true;
		}
	}

	// 导出
	if (col.exportInfo == null) {
		if (col.listInfo?.index != null) {
			col.exportInfo = {
				index: col.listInfo.index,
			};
		}
	}

	// editInfo
	if (col.editorInfo) {
		if (!col.editorInfo.title) {
			col.editorInfo.title = col.listInfo?.title?.toString() || col.title?.toString() || col.key;
		}
		if (col.editorInfo.width === undefined) {
			col.editorInfo.width = '100%';
		}
		if (col.editorInfo.position === undefined) {
			col.editorInfo.position = 'line';
		}
		if (col.editorInfo.inputType === undefined) {
			col.editorInfo.inputType = col.dbInfo.dataType;
		}
		if (col.key.toString().includes('.')) {
			col.editorInfo.isPost = false;
		} else if (col.editorInfo.isPost === undefined) {
			col.editorInfo.isPost = true;
		}
		if (col.editorInfo.display === undefined) {
			col.editorInfo.display = 'show';
		}
		if (col.editorInfo.options === undefined) {
			if (lodash.isArray(col.listInfo.options)) {
				col.editorInfo.options = col.listInfo.options;
			}
		}
		if (col.editorInfo.selectMode === 'multiple') {
			multipleCols.push(key);
			if (col.editorInfo.selectSplit === undefined) {
				col.editorInfo.selectSplit = ';';
			}
		}
		// editTempRecord[col.key!] = null;
		// editResetOption[col.key!] = true;
		// lodash.update(dataSource.editRecord, col.key!, () => null);
		// _editColsSort.push(col);
	}

	// ant col
	const antCol: TableColumnProps = {
		dataIndex: path,
		key: path.join('.'),
	};
	initSetAntCol(antCol, col);
	initWatchAntCol(key);

	return antCol;
};
/** col添加watch */
const initWatchAntCol = (key: string) => {
	watch(
		() => lodash.get(props.columns, key) as KaTableCol,
		col => {
			props.isDebug && console.log('触发watch', col.key);
			const antCol = antCols.value.find(_c => _c.key === col.key);
			if (!antCol) return;

			initSetAntCol(antCol, col);
		},
		{ deep: true }
	);
};
/** 设置col属性 */
const initSetAntCol = (antCol: TableColumnProps, col: KaTableCol) => {
	// props.isDebug && console.log('initSetAntCol', antCol.key);
	antCol.title = col.listInfo.title || col.title;
	antCol.align = col.listInfo.align;
	antCol.customCell = col.listInfo.customCell;
	antCol.customHeaderCell = col.listInfo.customHeaderCell;
	antCol.customRender = col.listInfo.customRender;
	antCol.ellipsis = col.listInfo.ellipsis;
	antCol.fixed = col.listInfo.fixed;
	antCol.maxWidth = col.listInfo.maxWidth;
	antCol.minWidth = col.listInfo.minWidth;
	antCol.resizable = col.listInfo.resizable;
	antCol.responsive = col.listInfo.responsive;

	// 排序
	setAntSortOrder(
		{ col: col.key as string, order: col.sortInfo?.order ?? null, index: col.sortInfo?.index ?? null },
		antCol
	);
	// console.log(JSON.stringify(col.sortInfo));
	// setAntSortOrder(col, antCol);
	// if (col.sortInfo) {
	// 	if (col.sortInfo.index != null) {
	// 		// antCol.defaultSortOrder = col.sortInfo.order;
	// 		antCol.sortOrder = col.sortInfo.order;
	// 		antCol.sorter = { multiple: col.sortInfo.index };
	// 	} else {
	// 		antCol.sortOrder = null;
	// 		antCol.sorter = { multiple: 1 };
	// 	}
	// } else {
	// 	antCol.sortOrder = null;
	// 	antCol.sorter = false;
	// }
	// console.log(JSON.stringify(antCol.sorter), antCol.sortOrder);

	// 筛选
	const filterColIndex = filterColumns.value.findIndex(c => c.col === col.key);
	if (props.toolbar.hasFilter !== false && col.filterInfo && col.filterInfo.isFilter) {
		antCol.customFilterDropdown = true;
		if (filterColIndex === -1) {
			filterColumns.value.push({
				col: col.key as string,
				title: col.title,
				type: col.dbInfo?.dataType,
				colLength: col.dbInfo?.colLength,
				dateFormat: col.dbInfo?.dateFormat,
				valOptions:
					col.filterInfo?.options || lodash.isArray(col.listInfo!.options)
						? (col.listInfo!.options as KaTableOptionItem[])
						: undefined,
			});
		}
		if (!antCol.filteredValue) antCol.filteredValue = [];
	} else {
		antCol.customFilterDropdown = false;
		antCol.filteredValue = [];
		if (filterColIndex !== -1) {
			filterColumns.value.splice(filterColIndex, 1);
		}
	}
	// TODO: 处理col
};
/** 初始化-处理props */
const initProps = () => {
	props.isDebug && console.groupCollapsed('initProps');

	if (props.initFilterConditions) {
		whereConditions = props.initFilterConditions;
	}

	if (props.toolbar.hasAdd == null) props.toolbar.hasAdd = true;
	if (props.toolbar.hasEdit == null) props.toolbar.hasEdit = true;
	if (props.toolbar.hasExport == null) props.toolbar.hasExport = true;
	if (props.toolbar.hasFilter == null) props.toolbar.hasFilter = true;
	if (props.toolbar.hasImport == null) props.toolbar.hasImport = true;
	if (props.toolbar.hasRefresh == null) props.toolbar.hasRefresh = true;
	if (props.toolbar.hasRemove == null) props.toolbar.hasRemove = true;
	if (props.toolbar.hasSort == null) props.toolbar.hasSort = true;

	// 字段
	for (const colName in props.columns) {
		initCols(props.columns[colName], [colName]);
	}
	//props.isDebug && console.log('antCols->', antCols.value);

	antCols.value.sort((a, b) => {
		const a_index = (lodash.get(props.columns, a.key!.toString()) as KaTableCol).listInfo.index;
		const b_index = (lodash.get(props.columns, b.key!.toString()) as KaTableCol).listInfo.index;
		return ((a_index == null) as any) - ((b_index == null) as any) || +(a_index > b_index) || -(a_index < b_index);
	});

	Object.keys(props.columns).forEach((colName) => {
	    if(props.columns[colName]._katableIsCol){
			insertCols.push(props.columns[colName] as KaTableCol);
		}
	});

	props.isDebug && console.groupEnd();
};
// #endregion

// #region toolbar
/** 点击刷新按钮 */
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
/** 点击新增按钮 */
const onToolbarAdd = async () => {
	props.isDebug && console.groupCollapsed('onToolbarAdd');
	loading.draw = true;

	try {
		initHandle.value='with_search';
		clearEditorItems();

		if (!(await eventHandle(props.onBeforeAdd))) return;

		tableStatus.value = 'Add';

		if (!(await eventHandle(props.onAfterAdd))) return;
	} catch (e: any) {
		showError(e);
	} finally {
		initHandle.value='';
		$form.value.clearValidate();
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
/** 取消新增、修改 */
const onDrawCancel = () => {
	tableStatus.value = null;
};
/** 提交新增、修改 */
// const onDrawSubmit = async () => {
// 	// if (tableStatus.value === 'Add') {
// 	// 	await addSubmit();
// 	// } else if (tableStatus.value === 'Edit') {
// 	// 	await editSubmit();
// 	// }
// };
/** 新增提交 */
const addSubmit = async () => {
	props.isDebug && console.groupCollapsed('addSubmit');
	loading.draw = true;
	try {
		if (!(await eventHandle(props.onPreAdd))) return;
		await $form.value.validate();
		if (!(await insertData())) return;
		if (!(await eventHandle(props.onPostAdd))) return;
		showAlert(props.language.addSuccess);
		tableStatus.value = 'List';
		await loadData();
	} catch {
	} finally {
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
/** 修改提交 */
const editSubmit = async () => {
	props.isDebug && console.groupCollapsed('editSubmit');
	loading.draw = true;
	try {
		if (!(await eventHandle(props.onPreEdit))) return;
		await $form.value.validate();
		if (!(await updateData())) return;
		if (!(await eventHandle(props.onPostEdit))) return;
		showAlert(props.language.editSuccess);
		tableStatus.value = 'List';
		await loadData();
	} catch {
	} finally {
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};

/** 点击修改按钮 */
const onToolbarEdit = async () => {
	props.isDebug && console.groupCollapsed('onToolbarEdit');
	loading.draw = true;

	try {
		if (dataSource.activeIndex == null) {
			showError(props.language.selectRow);
			return;
		}

		initHandle.value = 'only_search';
		setEditorItems();

		if (!(await eventHandle(props.onBeforeEdit))) return;

		$form.value.initValues();
		tableStatus.value = 'Edit';

		if (!(await eventHandle(props.onAfterEdit))) return;
	} catch (e: any) {
		showError(e);
	} finally {
		initHandle.value = '';
		$form.value.clearValidate();
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};

/** 点击排序按钮 */
const onToolbarSort = async () => {
	props.isDebug && console.groupCollapsed('onToolbarSort');
	loading.draw = true;

	try {
		if (!(await eventHandle(props.onBeforeSort))) return;

		sorterList.value = antCols.value
			.filter(antCol => antCol.sorter)
			.map(antCol => ({
				col: antCol.key as string,
				order: antCol.sortOrder!,
				title: antCol.title as string,
				disabled: antCol.sortOrder == null,
				index: antCol.sortOrder ? (antCol.sorter as any)?.multiple : null,
			}))
			.sort(
				(a, b) =>
					((a.index == null) as any) - ((b.index == null) as any) || +(a.index > b.index) || -(a.index < b.index)
			);

		tableStatus.value = 'Sort';

		if (!(await eventHandle(props.onAfterSort))) return;
	} catch {
	} finally {
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
const sortSubmit = async () => {
	props.isDebug && console.log('sortSubmit');
	loading.draw = true;

	try {
		if (!(await eventHandle(props.onPreSort))) return;
		const conditions = sorterList.value.map(l => ({ col: l.col, order: l.order as any }));
		setSorterOrders(conditions);
		setAntSortOrders(conditions);
		await loadData();
		if (!(await eventHandle(props.onPostSort))) return;
		tableStatus.value = 'List';
	} catch (error: any) {
		showError(error);
	} finally {
		loading.draw = false;
	}
};
/** 点击筛选按钮 */
const onToolbarFilter = async () => {
	props.isDebug && console.groupCollapsed('onToolbarFilter');
	loading.draw = true;

	try {
		if (!(await eventHandle(props.onBeforeFilter))) return;

		const _filterList = whereConditions.map(c => ({
			col: c.col,
			val: c.val,
			opt: c.opt,
			bool: c.bool,
		}));
		//filterList.value = _filterList;
		$filter.value?.setConditions(_filterList);

		tableStatus.value = 'Filter';
		// console.log(filterColumns.value)

		if (!(await eventHandle(props.onAfterFilter))) return;
	} catch {
	} finally {
		loading.draw = false;
		props.isDebug && console.groupEnd();
	}
};
const filterSubmit = async () => {
	props.isDebug && console.log('filterSubmit');
	loading.draw = true;

	try {
		if (!(await eventHandle(props.onPreFilter))) return;
		whereConditions = filterList.value.map(l => ({ col: l.col, bool: l.bool, opt: l.opt, val: l.val }));
		await loadData();
		if (!(await eventHandle(props.onPostFilter))) return;
		tableStatus.value = 'List';
	} catch (error: any) {
		showError(error);
	} finally {
		loading.draw = false;
	}
};
/** 点击导出 */
const onToolbarExport = async (isAll: boolean) => {
	props.isDebug && console.groupCollapsed('onToolbarExport');
	loading.list = true;

	try {
		if (!(await eventHandle(props.onBeforeExport))) return;

		await exportData(isAll);

		if (!(await eventHandle(props.onAfterExport))) return;
	} catch {
	} finally {
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};

/** toolbar事件传参 */
const eventHandle = async (event?: KaTableEventHandle) => {
	if (event) {
		const handleResult = await event(dataSource);
		if (!handleResult.isSuccess) {
			showError(handleResult.message || props.language.refreshError);
			return false;
		}

		if (handleResult.message) {
			const isConfirm = await new Promise<boolean>((resolve, _reject) => {
				showConfirm(resolve, handleResult.message!);
			});
			if (!isConfirm) {
				return false;
			}
		}
	}
	return true;
};

// #endregion

// #region 请求数据

/** 查询数据 */
const loadData = async () => {
	props.isDebug && console.groupCollapsed('loadData');

	try {
		loading.list = true;

		const sortConditions = createSortConditions();

		// const _whereConditions = [];
		let _whereConditions = whereConditions;
		if (props.frozenFilterConditions) {
			_whereConditions = [..._whereConditions, ...props.frozenFilterConditions];
		}

		// props.isDebug && console.log('sortInfo->', JSON.stringify(sortInfo));
		// // const _sortConditions = [] as (KaTableSortCondition & { index: number })[];
		// // for (const key in sortInfo) {
		// // 	if (sortInfo[key].order == null) continue;
		// // 	_sortConditions.push({ col: key, order: sortInfo[key].order!, index: sortInfo[key].index });
		// // }
		// // _sortConditions.sort((a, b) => a.index - b.index);

		props.isDebug && console.log('sortConditions->', JSON.stringify(sortConditions));
		props.isDebug && console.log('whereConditions->', JSON.stringify(_whereConditions));

		const res = await axios.post<KaTableSearchResponse>(
			props.url,
			qs.stringify({
				actNo: 'search',
				searchPar: JSON.stringify({
					sortConditions: sortConditions,
					whereConditions: _whereConditions,
					pageSize: pagination.pageSize,
					pageNum: pagination.current,
				} as KaTableSearchPar),
			})
		);

		const data = res.data;

		if (!data.isSuccess) {
			// showError(data.message || props.language.loadError);
			throw new Error(data.message || props.language.loadError);
		} else {
			for (const antCol of antCols.value) {
				const col = lodash.get(props.columns, antCol.key as string) as KaTableCol;

				if (col.dbInfo?.dataType === 'date') {
					for (const record of data.records) {
						lodash.update(record, col.key!, (v: string) => dayjs(v));
					}
				} else if (col.editorInfo?.selectMode === 'multiple') {
					for (const record of data.records) {
						lodash.update(record, col.key!, (v: string) => (v ? v.split(col.editorInfo!.selectSplit!) : v));
					}
				}
			}
		}

		dataSource.records = data.records || [];
		pagination.total = data.total || 0;
		dataSource.activeIndex = null;
		// clearEditItems();

		await eventHandle(props.onPostLoadData);
	} catch (error: any) {
		dataSource.records = [];
		pagination.total = 0;
		dataSource.activeIndex = null;
		showError(error);
		throw error;
	} finally {
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};
/** 导出数据 */
const exportData = async (isAll: boolean) => {
	props.isDebug && console.groupCollapsed('exportData');

	try {
		loading.list = true;

		await eventHandle(props.onPreExport);

		const sortConditions = createSortConditions();

		let _whereConditions = whereConditions;
		if (props.frozenFilterConditions) {
			_whereConditions = [..._whereConditions, ...props.frozenFilterConditions];
		}

		const _colsInfo = antCols.value
			.map(antCol => {
				const col = lodash.get(props.columns, antCol.key as string) as KaTableCol;
				if (col.exportInfo && col.exportInfo.index != null) {
					return {
						col: col.key,
						index: col.exportInfo.index,
						formula: col.exportInfo.formula,
						title: col.exportInfo.title || col.title,
						dateFormat: col.dbInfo!.dateFormat,
					};
				}
				return null;
			})
			.filter(v => v)
			.sort((a, b) => a!.index - b!.index)
			.map(c => ({
				col: c?.col,
				formula: c?.formula,
				title: c?.title as string,
				dateFormat: c?.dateFormat,
			})) as KaTableExportPar['cols'];

		props.isDebug && console.log('sortConditions->', JSON.stringify(sortConditions));
		props.isDebug && console.log('whereConditions->', JSON.stringify(_whereConditions));
		props.isDebug && console.log('cols->', JSON.stringify(_colsInfo));

		const res = await axios.post<KaTableResponse>(
			props.url,
			qs.stringify({
				actNo: 'export',
				exportPar: JSON.stringify({
					sortConditions: sortConditions,
					whereConditions: _whereConditions,
					cols: _colsInfo,
					pageSize: isAll ? 0 : pagination.pageSize,
					pageNum: pagination.current,
					fileName: `${props.tableTitle}(${dayjs().format('YYYYMMDDHHmmss')})`,
				} as KaTableExportPar),
			}),
			{
				responseType: 'blob',
			}
		);

		// console.log(res, res.headers['file-name']);

		if (res.headers['content-type']?.toString().includes('application/json')) {
			throw JSON.parse(await (res.data as any).text()).Message || props.language.exportError;
		}

		const file = new Blob([res.data as any], {
			type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
		});
		$export.value.href = URL.createObjectURL(file);
		$export.value.download = props.exportFileName ? props.exportFileName() : res.headers['file-name'];
		//$export.setAttribute('download', `考勤-${dayjs().format('MMDDHHmmss')}.xlsx`);
		$export.value.click();

		await eventHandle(props.onPostLoadData);
	} catch (error: any) {
		showError(error);
		throw error;
	} finally {
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};

/** 新增数据 */
const insertData = async () => {
	props.isDebug && console.log('insertData');

	const record = {} as {[key:string]:any};

	for(const col of insertCols){
		record[col.key!] = dataSource.editRecord[col.key!];
		if(col.editorInfo?.inputType === 'select'){
			record[col.key!] = record[col.key!].join(col.editorInfo.selectSplit);
		}
	}

	const res = await axios.post<KaTableResponse>(
		props.url,
		qs.stringify({
			actNo: 'insert',
			record: JSON.stringify(record),
		})
	);
	if (!res.data.isSuccess) {
		showError(res.data.message || props.language.addError);
		return false;
	}
	return true;
};
/** 修改数据 */
const updateData = async () => {
	props.isDebug && console.log('updateData');

	// const newData = {} as { [key: string]: any };
	// const oldData = {} as { [key: string]: any };
	// const curRecord = dataSource.records[dataSource.activeIndex!];
	// for (const key in dataSource.editRecord) {
	// 	if (dataSource.editRecord[key] !== curRecord[key]) {
	// 		newData[key] = dataSource.editRecord[key];
	// 		oldData[key] = curRecord[key];
	// 	} else {
	// 		if (pkCols.value?.some(col => col.key === key)) {
	// 			oldData[key] = curRecord[key];
	// 		}
	// 	}
	// }

	// if (Object.keys(newData).length === 0) {
	// 	showError(props.language.noChange);
	// 	return false;
	// }

	// const res = await axios.post<KaTableResponse>(
	// 	props.url,
	// 	qs.stringify({
	// 		actNo: 'update',
	// 		oldData: JSON.stringify(oldData),
	// 		newData: JSON.stringify(newData),
	// 	})
	// );
	// if (!res.data.isSuccess) {
	// 	showError(res.data.message || props.language.addError);
	// 	return false;
	// }
	return true;
};

// #endregion

// #region 弹窗

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
const showAlert = (message: any) => {
	Modal.info({ title: lodash.isString(message) ? message : message.message });
};

// #endregion

// #region 生命周期
onMounted(() => {
	if (props.autoLoad) {
		try {
			loadData();
		} catch (e: any) {
			showError(e);
		}
	}
});
onBeforeMount(() => {
	initProps();
});
// #endregion
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
