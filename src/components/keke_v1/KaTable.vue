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
			v-bind="$props"
			class="ka-table"
			bordered
			:custom-row="customRow"
			:row-class-name="rowClassName"
			:pagination="dataSource.page.pageSize === 0 ? false : dataSource.page"
			@change="tableChange"
			:loading="loading.list"
			:data-source="dataSource.records"
			:showSorterTooltip="false"
			:columns="listCols"
		>
			<template v-for="(_v, k) in $slots" v-slot:[k] :key="k">
				<slot :name="k"></slot>
			</template>
			<template #bodyCell="{ text, column, index, record }">
				<slot name="bodyItem" :column="column" :index="index" :text="text" :record="record">{{
					formatBodyItem(text, column)
				}}</slot>
			</template>

			<template #title>
				<!-- 标题栏 -->
				<a-flex class="ka-table-title" justify="space-between" align="center">
					<!-- 标题 -->
					<a-typography-title :level="4">{{ props.tableTitle }}</a-typography-title>
					<!-- 工具栏 -->
					<div>
						<a-space>
							<a-space-compact :size="props.size">
								<a-tooltip v-if="props.toolbar.hasRefresh" :title="props.language.toolbarRefresh">
									<a-button @click="onToolbarRefresh">
										<ReloadOutlined />
									</a-button>
								</a-tooltip>
							</a-space-compact>
							<a-space-compact :size="props.size">
								<a-tooltip v-if="props.toolbar.hasSearch" :title="props.language.toolbarSearch">
									<a-button :style="{ color: whereConditions.length ? props.theme : null }">
										<SearchOutlined />
									</a-button>
								</a-tooltip>
								<a-tooltip v-if="props.toolbar.hasExport" :title="props.language.toolbarExport">
									<a-button>
										<DownloadOutlined />
									</a-button>
								</a-tooltip>
								<a-tooltip v-if="props.toolbar.hasSort" :title="props.language.toolbarSort">
									<a-button :style="{ color: sortConditions.length ? props.theme : null }">
										<SortAscendingOutlined />
									</a-button>
								</a-tooltip>
							</a-space-compact>
							<a-space-compact :size="props.size">
								<a-tooltip v-if="props.toolbar.hasAdd" :title="props.language.toolbarAdd">
									<a-button @click="onToolbarAdd">
										<PlusOutlined />
									</a-button>
								</a-tooltip>
								<a-tooltip v-if="props.toolbar.hasImport" :title="props.language.toolbarImport">
									<a-button>
										<UploadOutlined />
									</a-button>
								</a-tooltip>
								<a-tooltip v-if="props.toolbar.hasEdit" :title="props.language.toolbarEdit">
									<a-button @click="onToolbarEdit" :disabled="dataSource.activeIndex === null">
										<EditOutlined />
									</a-button>
								</a-tooltip>
								<a-tooltip v-if="props.toolbar.hasDelete" :title="props.language.toolbarDelete">
									<a-button :disabled="dataSource.activeIndex === null">
										<DeleteOutlined />
									</a-button>
								</a-tooltip>
							</a-space-compact>
							<slot name="toolbar" :col1="test"></slot>
						</a-space>
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
							<a-button @click="onSearchConfirm(setSelectedKeys, selectedKeys, confirm, column)" type="primary">
								{{ props.language.toolbarSearch }}
							</a-button>
							<a-button @click="onSearchReset(clearFilters, confirm, column)">{{ props.language.reset }}</a-button>
						</a-flex>
					</a-space>
				</div>
			</template>
			<template #customFilterIcon="{ filtered }">
				<search-outlined :style="{ color: filtered ? props.theme : undefined }" />
			</template>
			<template #headerCell="{ column }">
				<template v-if="column.isSort">
					<a-flex justify="space-between" align="center">
						<span>{{ column.title }}</span>
						<a-typography-text type="secondary">{{ sortInfo[column.key].index || '' }}</a-typography-text>
					</a-flex>
				</template>
			</template>
		</a-table>
	</a-config-provider>
	<a-drawer
		:title="
			tableStatus === 'Add'
				? props.addTitle || props.language.toolbarAdd
				: props.editTitle || props.language.toolbarEdit
		"
		:width="editDialogWidth"
		:open="tableStatus === 'Add' || tableStatus === 'Edit'"
		@close="onEditCancel"
		class="ka-table-drawer"
		:closable="!loading.edit"
		:mask-closable="false"
		:force-render="true"
	>
		<template #extra>
			<a-space>
				<a-button type="primary" @click="onEditSubmit" :loading="loading.edit">{{ props.language.submit }}</a-button>
			</a-space>
		</template>
		<a-spin :spinning="loading.edit">
			<a-form ref="$form" :model="editTempRecord" layout="vertical">
				<!-- colRow [ [col1, col2] ], [ [col1], [col2] ] -->
				<template v-for="(colRow, _index) in editInfo" :key="_index">
					<!-- [ [col1] ] line -->
					<template v-if="colRow.length === 1 && colRow[0].length === 1">
						<a-form-item
							:label="colRow[0][0].editInfo!.title"
							:style="{ width: colRow[0][0].editInfo!.width,display: colRow[0][0].editInfo!.display==='hide' }"
							:required="colRow[0][0].isRequired"
							:rules="colRow[0][0].editInfo!.rules"
							:name="colRow[0][0].key"
						>
							<ka-input
								v-model="editTempRecord[colRow[0][0].key!]"
								:column="colRow[0][0]"
								:data-source="dataSource"
								v-bind="colRow[0][0].editInfo!.attrs"
								:manual-reset-option="editResetOption[colRow[0][0].key!]"
							>
							</ka-input>
						</a-form-item>
					</template>
					<!-- colGroup [col1, col2] 、 [col1], [col2] -->
					<template v-else>
						<div class="edit-item-inline-block">
							<template v-for="(colGroup, _colIndex) in colRow" :key="_colIndex">
								<!-- [col1] inline -->
								<template v-if="colGroup.length === 1">
									<a-form-item
										:label="colGroup[0].editInfo!.title"
										:style="{ width: colGroup[0].editInfo!.width,display: colGroup[0].editInfo!.display==='hide'}"
										:required="colGroup[0].isRequired"
										:rules="colGroup[0].editInfo!.rules"
										:name="colGroup[0].key"
									>
										<ka-input
											v-model="editTempRecord[colGroup[0].key!]"
											:column="colGroup[0]"
											:data-source="dataSource"
											v-bind="colGroup[0].editInfo!.attrs"
											:manual-reset-option="editResetOption[colGroup[0].key!]"
										>
										</ka-input>
									</a-form-item>
								</template>
								<!-- [col1, col2] cling -->
								<a-form-item
									v-else
									:label="colGroup[0].editInfo!.title"
									:required="colGroup[0].isRequired"
									:style="{display: colGroup[0].editInfo!.display==='hide'}"
									:rules="colGroup[0].editInfo!.rules"
									:name="colGroup[0].key"
								>
									<a-input-group compact>
										<!-- <a-input
												v-for="(col, _i) in colGroup"
												:key="_i"
												size="normal"
												:style="{ width: col.editInfo!.width, display: col.editInfo!.display==='hide' }"
												v-model:value="dataSource.editRecord[col.key!]"
												:disabled="col.editInfo!.display === 'readonly'"
											>
											</a-input> -->
										<ka-input
											v-for="(col, _i) in colGroup"
											:key="_i"
											:style="{ width: col.editInfo!.width, display: col.editInfo!.display==='hide' }"
											v-model="editTempRecord[col.key!]"
											:column="col"
											:data-source="dataSource"
											v-bind="col.editInfo!.attrs"
											:manual-reset-option="editResetOption[col.key!]"
										></ka-input>
									</a-input-group>
								</a-form-item>
							</template>
						</div>
					</template>
				</template>
			</a-form>
		</a-spin>
	</a-drawer>
	<a-button @click="testBtn">test</a-button>
	<a-button @click="testBtn2">test2</a-button>
</template>

<script setup lang="ts">
// import { useAttrs } from 'vue';
import { computed, onBeforeMount, onMounted, reactive, ref, watch } from 'vue';
import KaInput from './KaInput.vue';
import dayjs from 'dayjs';
import {
	KaTableCol,
	KaTableCols,
	KaTableEditCol,
	KaTableEventHandle,
	KaTablePage,
	KaTableRecord,
	KaTableResponse,
	KaTableRowRecord,
	KaTableSearchPar,
	KaTableSortCondition,
	KaTableSortItem,
	KaTableStatus,
	KaTableWhereCondition,
	isArray,
	isBool,
	isNumber,
	isObject,
	isString,
	kaTableProps,
} from './type';
import {
	SearchOutlined,
	ReloadOutlined,
	SortAscendingOutlined,
	PlusOutlined,
	EditOutlined,
	DeleteOutlined,
	DownloadOutlined,
	UploadOutlined,
} from '@ant-design/icons-vue';
import axios from 'axios';
import qs from 'qs';
import { Modal, TableProps, theme, FormInstance } from 'ant-design-vue';
import { Key, SortOrder, SorterResult, ColumnType, FilterValue } from 'ant-design-vue/es/table/interface';
import lodash from 'lodash';
import { NamePath } from 'ant-design-vue/es/form/interface';

const testBtn = () => {
	// console.log(props.columns);
	// props.columns[0].sorter!.multiple = 1;
	// props.columns[0].title = 'xx';
	// setSorterOrders([{ columnKey: 'name', order: 'ascend' }]);
	// props.columns[0].filteredValue = ['xx'];
	// setFilter('name', ['yy']);
	loadData();
};
const testBtn2 = () => {
	// console.log(props.columns);
	props.columns[0].sortOrder = null;
};

const test = ref(1);
//const xx = props.columns[0]

// #region data

const props = defineProps(kaTableProps());

const { useToken } = theme;
const { token } = useToken();
const activeRowColor = props.theme ? props.theme + '55' : token.value.colorPrimaryBg;
const colsColor = props.theme ? props.theme + '33' : token.value.colorFillAlter;
const titleColor = props.theme || token.value.colorFillAlter;

/** form ref */
const $form = ref<FormInstance>(null as unknown as FormInstance);

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

/** 显示列 */
const listCols = ref<KaTableCols<any>>();
/** 编辑列 */
const editCols = ref<KaTableCols<any>>();
/** 主键列 */
const pkCols = ref<KaTableCols<any>>();

/** 数据源 */
const dataSource: KaTableRecord = reactive({
	records: [],
	activeIndex: null,
	editRecord: {},
	page: {
		current: 1,
		pageSize: props.pageSize,
		total: 0,
		showSizeChanger: false,
		showTotal: (total: number, range: number[]) => `${range[0]}-${range[1]} of ${total} items`,
	},
});
/** 编辑框中v-model使用 */
const editTempRecord: { [index: string]: any } = reactive({});
/** 编辑框手工触发选项标识 */
const editResetOption: { [index: string]: boolean } = reactive({});

/** 操作状态 */
const tableStatus = ref<KaTableStatus>();

/** 排序信息 */
const sortInfo = reactive({} as { [index: string]: KaTableSortItem });
/** 编辑框信息 */
const editInfo = ref<KaTableCol<any>[][][]>();
/** 筛选条件 */
let whereConditions: KaTableWhereCondition[] = [];
let sortConditions = [] as (KaTableSortCondition & { index: number })[];

/** 加载状态 */
const loading = reactive({
	list: false,
	edit: false,
});

/** 新增窗口宽度 */
const editDialogWidth = ref<number | string>();

// #endregion

// #region methods

/** 自定义行事件 */
const customRow = (preRecord: KaTableRowRecord, index: number | undefined) => {
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
const rowClassName = (record: any) => {
	// console.log('rowClassName',record);
	return record.isActive ? 'ka-table-selected-row' : '';
};
const formatBodyItem = (text: any, column: KaTableCol<any>) => {
	if (column.dbInfo?.colType === 'date') {
		return dayjs(text).format(column.dbInfo.dateFormat);
	}
	return text;
};
// const setObjectPropVal = (prop: string, obj: { [key: string]: any }, val: any) => {
// 	const arr = prop.split('.');
// 	if (arr.length === 1) {
// 		obj[prop] = val;
// 		return;
// 	}

// 	let col = obj;
// 	for (let i = 0; i < arr.length - 1; i++) {
// 		col = col[arr[i]];
// 	}
// 	col[arr[arr.length - 1]] = val;
// };

/** 设置排序值,外部控制排序请用setSorterOrders */
const setSorterOrder = (colKey: Key, order: SortOrder, index: number) => {
	// console.log(colKey,order)
	const col = props.columns.find(c => c.key === colKey);
	if (col) {
		sortInfo[colKey].order = col.sortOrder = order || null;
		sortInfo[colKey].index = index;
		if (!index) {
			col.sorter = true;
		} else {
			if (isObject(col.sorter)) {
				col.sorter = { multiple: index };
			} else if (isBool(col.sorter)) {
				col.sorter = true;
			}
		}
		return col;
	}
	return null;
};
/** 设置排序值 */
const setSorterOrders = (orders: SorterResult[]) => {
	const sortedKeys = {} as { [index: string]: SorterResult };
	let isSortedMult = true;
	for (const s of orders) {
		sortedKeys[s.columnKey!] = s;
		if (!sortInfo[s.columnKey!].index) {
			isSortedMult = false;
		}
	}
	for (const key in sortInfo) {
		if (sortedKeys[key]) {
			setSorterOrder(key, sortedKeys[key].order || null, isSortedMult ? sortInfo[key].index : 0);
		} else {
			setSorterOrder(key, null, isSortedMult ? sortInfo[key].index : 0);
		}
	}

	setSorterConditions();
};
const setSorterConditions = () => {
	sortConditions = [];
	for (const key in sortInfo) {
		if (sortInfo[key].order == null) continue;
		sortConditions.push({ col: key, order: sortInfo[key].order!, index: sortInfo[key].index });
	}
	sortConditions.sort((a, b) => a.index - b.index);
};
/** 设置筛选值 */
// const setFilters = (conditions: KaTableWhereCondition[]) => {
// 	whereConditions = conditions;
// 	for (const condition of whereConditions) {
// 		if (condition.col) {
// 			setFilter(condition.col, condition.val);
// 		} else if (condition.children) {
// 			setFilters(condition.children);
// 		}
// 	}
// };
const changeAntFilters = (filters: Record<string, FilterValue | null>) => {
	whereConditions = [];
	for (const key in filters) {
		addFilter(key, filters[key]);
	}
};
const addFilter = (key: string, filter: FilterValue | null) => {
	if (filter && filter.length) {
		const col = props.columns.find(c => c.key === key);
		let opt: KaTableWhereCondition['opt'] = 'eq';
		if (col?.dbInfo?.colType === 'string') {
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
};
/** 确认查询 */
const onSearchConfirm = (setSelectedKeys, selectedKeys: any[], confirm: Function, column: ColumnType) => {
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
/** 重置查询 */
const onSearchReset = (clearFilters: Function, confirm: Function, column: ColumnType) => {
	clearFilters();
	column.filteredValue = [];
	confirm();
};
/** 触发表格查询 */
const tableChange: TableProps['onChange'] = (pagination: KaTablePage, filters, sorter, extra) => {
	// console.log('filters', filters);
	// console.log('sorters', sorter);
	// console.log('extra', extra);
	// loading.list = true;

	// 分页
	dataSource.page.current = pagination.current;

	// 排序
	setSorterOrders((sorter as any).length ? (sorter as SorterResult[]) : [sorter as SorterResult]);

	//筛选
	changeAntFilters(filters);

	//test
	//dataSource.value = [];
	loadData();
};
/** 填入编辑框值 */
const setEditItems = () => {
	if (tableStatus.value === 'Edit') {
		if (dataSource.activeIndex !== null) {
			const activeRecord = dataSource.records[dataSource.activeIndex];
			for (const key in editTempRecord) {
				editTempRecord[key] = lodash.get(activeRecord, key);
			}
		}
	} else {
		clearEditItems();
	}
};

/** 初始化-处理props */
const initProps = () => {
	props.isDebug && console.groupCollapsed('initProps');

	let _listCols = [];
	let _editCols = [];
	let _pkCols = [];
	let _editColsSort: KaTableCol<any>[] = [];

	if (props.initWhereConditions) {
		whereConditions = props.initWhereConditions;
	}

	// 字段
	for (const col of props.columns) {
		// dataindex
		if (!col.dataIndex) {
			throw `列(${col.title})未设置(dataIndex)`;
		}

		// key
		if (!col.key) {
			if (isArray(col.dataIndex)) {
				col.key = (<string[]>col.dataIndex).join('.');
			} else {
				col.key = col.dataIndex.toString();
			}
		}

		// pk
		if (col.dbInfo?.isPk) {
			_pkCols.push(col);
		}

		// dbInfo
		if (!col.dbInfo) {
			col.dbInfo = {};
		}
		if (!col.dbInfo.colType) {
			col.dbInfo.colType = 'string';
		}
		if (col.dbInfo.colType === 'date') {
			if (col.dbInfo.dateFormat === undefined) {
				col.dbInfo.dateFormat = 'YYYY/MM/DD';
			}
		}

		// 排序
		if (col.initSorter) {
			sortInfo[col.key] = col.initSorter;
			// antd
			col.defaultSortOrder = col.initSorter.order;
			col.sorter = { multiple: col.initSorter.index };

			col.isSort = true;
		}
		if (col.isSort) {
			if (!sortInfo[col.key]) {
				sortInfo[col.key] = { index: 0, order: null };
				col.defaultSortOrder = null;
				col.sorter = true;
			}
		} else {
			col.defaultSortOrder = null;
			col.sorter = false;
		}

		// 筛选
		// antd
		if (col.isFilt) {
			col.customFilterDropdown = true;
			if (!col.filteredValue) {
				col.filteredValue = [];
			} else {
				if (whereConditions.length) {
					col.filteredValue = [];
				} else {
					addFilter(col.key.toString(), col.filteredValue);
				}
			}
		} else {
			col.customFilterDropdown = false;
			col.filteredValue = null;
		}

		//listInfo
		if (col.listInfo) {
			if (col.listInfo.index === undefined) {
				throw `列(${col.dataIndex})未设置(listInfo.index)`;
			}
			if (!col.listInfo.title) {
				col.listInfo.title = col.title?.toString() || col.dataIndex.toString();
			}
			if (col.listInfo.width === undefined) {
				col.listInfo.width = 100;
			}
			_listCols.push(col);
		}

		// editInfo
		if (col.editInfo) {
			if (col.editInfo.index === undefined) {
				throw `列(${col.dataIndex})未设置(editInfo.index)`;
			}
			if (!col.editInfo.title) {
				col.editInfo.title = col.listInfo?.title || col.title?.toString() || col.dataIndex.toString();
			}
			if (col.editInfo.width === undefined) {
				col.editInfo.width = '100%';
			}
			if (col.editInfo.position === undefined) {
				col.editInfo.position = 'line';
			}
			if (col.editInfo.inputType === undefined) {
				col.editInfo.inputType = col.dbInfo.colType;
			}
			if (col.key.toString().includes('.')) {
				col.editInfo.isPost = false;
			} else if (col.editInfo.isPost === undefined) {
				col.editInfo.isPost = true;
			}
			if (col.editInfo.display === undefined) {
				col.editInfo.display = 'show';
			}
			editTempRecord[col.key!] = null;
			editResetOption[col.key!] = true;
			lodash.update(dataSource.editRecord, col.key!, () => null);
			_editColsSort.push(col);
		}

		// TODO: 处理其他属性

		props.isDebug && console.groupEnd();
	}

	pkCols.value = _pkCols;
	// listCols排序
	listCols.value = _listCols.sort((a, b) => a.listInfo!.index - b.listInfo!.index);
	// editCols排序
	// [
	// 	[[col1 inline],[col1,col2 cling],[col3 inline]]
	// ]
	_editColsSort.sort((a, b) => a.editInfo!.index - b.editInfo!.index);
	editCols.value = _editColsSort;
	if (_editColsSort.length) {
		let preCol: KaTableCol<any>[][] | null = null;
		for (let col of _editColsSort) {
			if (col.editInfo!.position === 'line' || preCol === null) {
				preCol = [[col]];
				_editCols.push(preCol);
			} else if (col.editInfo!.position === 'inline') {
				preCol.push([col]);
			} else if (col.editInfo!.position === 'cling') {
				preCol[preCol.length - 1].push(col);
			}
		}
		editInfo.value = _editCols;
	}
	//watch editTempRecord
	const _editRecordMethods: { key: string; fn: () => any }[] = [];
	const _edtiTempRecordMethods: { key: string; fn: () => any }[] = [];
	for (const key of Object.keys(editTempRecord)) {
		_edtiTempRecordMethods.push({ key, fn: () => editTempRecord[key] });
		_editRecordMethods.push({ key, fn: () => lodash.get(dataSource.editRecord, key) });
	}
	for (const m of _editRecordMethods) {
		watch(m.fn, v => {
			editResetOption[m.key] = !editResetOption[m.key];
			editTempRecord[m.key] = v;
		});
	}
	for (const m of _edtiTempRecordMethods) {
		watch(m.fn, v => {
			// setObjectPropVal(m.key, dataSource.editRecord, v);
			lodash.set(dataSource.editRecord, m.key, v);
		});
	}

	setSorterConditions();
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
	} finally {
		loading.list = false;
		props.isDebug && console.groupEnd();
	}
};
/** 点击新增按钮 */
const onToolbarAdd = async () => {
	props.isDebug && console.groupCollapsed('onToolbarAdd');
	loading.edit = true;

	try {
		if (!(await eventHandle(props.onBeforeAdd))) return;

		editDialogWidth.value = resetEditDialogWidth();
		tableStatus.value = 'Add';

		setEditItems();

		if (!(await eventHandle(props.onAfterAdd))) return;
	} finally {
		loading.edit = false;
		props.isDebug && console.groupEnd();
	}
};
/** 取消新增、修改 */
const onEditCancel = () => {
	tableStatus.value = null;
};
/** 提交新增、修改 */
const onEditSubmit = async () => {
	if (tableStatus.value === 'Add') {
		await addSubmit();
	} else if (tableStatus.value === 'Edit') {
		await editSubmit();
	}
};
/** 新增提交 */
const addSubmit = async () => {
	props.isDebug && console.groupCollapsed('addSubmit');
	loading.edit = true;
	try {
		if (!(await eventHandle(props.onPreAdd))) return;
		if (!(await insertData())) return;
		if (!(await eventHandle(props.onPostAdd))) return;
		showAlert(props.language.addSuccess);
		tableStatus.value = null;
		await loadData();
	} finally {
		loading.edit = false;
		props.isDebug && console.groupEnd();
	}
};
/** 修改提交 */
const editSubmit = async () => {
	props.isDebug && console.groupCollapsed('editSubmit');
	loading.edit = true;
	try {
		if (!(await eventHandle(props.onPreEdit))) return;
		if (!(await updateData())) return;
		if (!(await eventHandle(props.onPostEdit))) return;
		showAlert(props.language.editSuccess);
		tableStatus.value = null;
		await loadData();
	} finally {
		loading.edit = false;
		props.isDebug && console.groupEnd();
	}
};

/** 点击修改按钮 */
const onToolbarEdit = async () => {
	props.isDebug && console.groupCollapsed('onToolbarEdit');
	loading.edit = true;

	try {
		if (!(await eventHandle(props.onBeforeEdit))) return;

		editDialogWidth.value = resetEditDialogWidth();
		tableStatus.value = 'Edit';

		setEditItems();

		if (!(await eventHandle(props.onAfterEdit))) return;
	} finally {
		loading.edit = false;
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
const showError = (message: string) => {
	Modal.error({ title: message });
};
/** 提示框 */
const showAlert = (message: string) => {
	Modal.info({ title: message });
};

/** 重新计算编辑窗口宽度 */
const resetEditDialogWidth = () => {
	let pw = 0;
	if (isNumber(props.editDialogWidth)) {
		pw = props.editDialogWidth as number;
	} else if (isString(props.editDialogWidth)) {
		if (parseInt(props.editDialogWidth as string) === +(props.editDialogWidth as string)) {
			pw = parseInt(props.editDialogWidth as string);
		}
	} else {
		return props.editDialogWidth;
	}

	if (window.innerWidth < pw) {
		return '100%';
	}
	return pw;
};

/** 清空编辑框数据 */
const clearEditItems = () => {
	for (const key in editTempRecord) {
		editTempRecord[key] = null;
	}
};

// #endregion

// #region 请求数据

/** 查询数据 */
const loadData = async () => {
	props.isDebug && console.groupCollapsed('loadData');

	let _whereConditions = whereConditions;
	if (props.frozenWhereConditions) {
		_whereConditions = [..._whereConditions, ...props.frozenWhereConditions];
	}

	props.isDebug && console.log('sortInfo->', JSON.stringify(sortInfo));
	// const _sortConditions = [] as (KaTableSortCondition & { index: number })[];
	// for (const key in sortInfo) {
	// 	if (sortInfo[key].order == null) continue;
	// 	_sortConditions.push({ col: key, order: sortInfo[key].order!, index: sortInfo[key].index });
	// }
	// _sortConditions.sort((a, b) => a.index - b.index);

	// props.isDebug && console.log('sortConditions->', JSON.stringify(_sortConditions));
	props.isDebug && console.log('whereConditions->', JSON.stringify(_whereConditions));

	const res = await axios.post<KaTableResponse>(
		props.url,
		qs.stringify({
			actNo: 'search',
			searchPar: JSON.stringify({
				sortConditions: sortConditions,
				whereConditions: _whereConditions,
				pageSize: dataSource.page.pageSize,
				pageNum: dataSource.page.current,
			} as KaTableSearchPar),
		})
	);

	const data = res.data;

	if (!data.isSuccess) {
		showError(data.message || props.language.loadError);
	} else {
		for (const col of props.columns) {
			if (col.dbInfo?.colType === 'date') {
				for (const record of data.records) {
					lodash.update(record, col.key!, (v: string) => dayjs(v));
				}
			}
		}
	}

	dataSource.records = res.data.records || [];
	dataSource.page.total = res.data.total || 0;
	dataSource.activeIndex = null;
	clearEditItems();

	await eventHandle(props.onPostLoadData);
	// console.log(dataSource);
	props.isDebug && console.groupEnd();
};

/** 新增数据 */
const insertData = async () => {
	props.isDebug && console.log('insertData');
	const res = await axios.post<KaTableResponse>(
		props.url,
		qs.stringify({
			actNo: 'insert',
			record: JSON.stringify(dataSource.editRecord),
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

	const newData = {} as { [key: string]: any };
	const oldData = {} as { [key: string]: any };
	const curRecord = dataSource.records[dataSource.activeIndex!];
	for (const key in dataSource.editRecord) {
		if (dataSource.editRecord[key] !== curRecord[key]) {
			newData[key] = dataSource.editRecord[key];
			oldData[key] = curRecord[key];
		} else {
			if (pkCols.value?.some(col => col.key === key)) {
				oldData[key] = curRecord[key];
			}
		}
	}

	if (Object.keys(newData).length === 0) {
		showError(props.language.noChange);
		return false;
	}

	const res = await axios.post<KaTableResponse>(
		props.url,
		qs.stringify({
			actNo: 'update',
			oldData: JSON.stringify(oldData),
			newData: JSON.stringify(newData),
		})
	);
	if (!res.data.isSuccess) {
		showError(res.data.message || props.language.addError);
		return false;
	}
	return true;
};

// #endregion

// #region 生命周期
onMounted(() => {
	loadData();
});
onBeforeMount(() => {
	initProps();
});
// #endregion

// var attrs = useAttrs() as KaTableAttr;
// console.log(attrs);
// console.log(props);
</script>

<style scoped>
/* .ka-table{border: 1px solid #eee;} */
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
