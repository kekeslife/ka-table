<script setup lang="ts">
import { onBeforeMount, onMounted, reactive, ref } from 'vue';
import dayjs from 'dayjs';
import { KaTableCols, KaTableDataSource, KaTablePropsToolbar } from '.';
import KaTable from './KaTable.vue';
import { KaSorterCondition } from '../ka_sorter';
import zhCN from 'ant-design-vue/es/locale/zh_CN';
import 'dayjs/locale/zh-cn';
import { Locale } from 'ant-design-vue/es/locale';
import { KaEditorItemOption } from '../ka_editor';

const statusOptions = [] as KaEditorItemOption[];

const columns = reactive<KaTableCols>({
	code: {
		title: '公文号',
		dbInfo: {
			isPk: true,
		},
		listInfo: {
			index: 0,
			width: '100px',
		},
		sortInfo: {
			index: 0,
		},
		filterInfo: {
			isFilter: true,
		},
		editorInfo: {
			index: 1,
			width: '150px',
			rules: [{ required: true, message: '请输入公文号' }],
			// display: 'readonly',
		},
		importInfo: {
			index: 1,
			title: '公文号',
		},
	},
	item: {
		title: '序号',
		dbInfo: {
			isPk: true,
			dataType: 'number',
		},
		listInfo: {
			index: 1,
		},
		sortInfo: {
			index: 1,
		},
		editorInfo: {
			index: 2,
			width: '150px',
			position: 'inline',
			rules: [
				{
					validator: async (_rule, _value) => {
						try {
							await $table.value?.validateFields('code');
						} finally {
							return Promise.resolve();
						}
					},
				},
			],
		},
		importInfo: {
			index: 2,
			title: '序号',
		},
	},
	deptNo: {
		title: '部门编号',
		listInfo: {
			index: 2,
		},
		filterInfo: {
			isFilter: true,
		},
		editorInfo: {
			index: 3,
			width: '150px',
		},
		importInfo: {
			index: 3,
			title: '部门',
		},
	},
	remark: {
		title: '备注',
		listInfo: {
			index: 5,
		},
		editorInfo: {
			index: 6,
			componentType: 'select',
			selectMode: 'multiple',
			options: [
				{ label: 'xx1', value: 'xx1' },
				{ label: 'xx2', value: 'xx2' },
				{ label: 'xx3', value: 'xx3' },
				{ label: 'xx4', value: 'xx4' },
				{ label: 'xx', value: 'xx' },
			],
		},
	},
	modifyDatetime: {
		title: '修改时间',
		dbInfo: {
			dataType: 'date',
		},
	},
	tempNum: {
		title: '临时编号',
		editorInfo: {
			index: 7,
			isPost: false,
		},
	},
	docFlow: {
		code: {
			title: '公文号2',
		},
		status: {
			title: '状态',
			listInfo: {
				index: 3,
				options: [
					{ label: '开立', value: '0' },
					{ label: '流程中', value: '1' },
					{ label: '结案', value: '2' },
					{ label: '作废', value: '3' },
				],
			},
			editorInfo: {
				index: 4,
				width: '150px',
				componentType: 'select',
				options: statusOptions,
				onAfterChange: async (value: any, option?: any) => {
					// data.code = 'xxx';
					// $table.value!.validateFields([['code']]);
					console.log('onAfterChange', value, option);
					// return {isSuccess:true,message:'xxx'};
				},
			},
		},
		title: {
			title: '标题',
			listInfo: {
				index: 1.1,
			},
			editorInfo: {
				index: 1.1,
				width: '150px',
				position: 'cling',
			},
		},
		modifyDatetime: {
			title: '修改时间',
			dbInfo: {
				dataType: 'date',
			},
			listInfo: {
				index: 4,
			},
			sortInfo: {
				index: 3,
			},
			editorInfo: {
				index: 5,
				width: '150px',
				position: 'inline',
				// display:'readonly',
			},
		},
	},
});

const sorterConditions = ref<KaSorterCondition[]>([
	{ key: 'code', order: 'ascend', index: 1 },
	{ key: 'item', order: null, index: null },
	// { key: 'docFlow.modifyDatetime', order: null, index: null },
]);

const toolbar = ref<KaTablePropsToolbar | undefined>({
	hasFilter: true,
});
const locale = ref<Locale>();

const testInit = () => {
	toolbar.value = {
		hasRemove: false,
	};
	columns.code.listInfo!.title = '公文号码';
	$table.value?.reinit();
};
const closeSorter = () => {
	// columns.code.sortInfo = undefined;
	// columns.item.sortInfo = undefined;
	// columns.docFlow.modifyDatetime.sortInfo = undefined;
	toolbar.value!.hasSort = false;
	$table.value?.reinit();
};
const closeFilter = () => {
	// columns.modifyDatetime.filterInfo.isFilter = false;
	// columns.docFlow.code.filterInfo.isFilter = false;
	// columns.docFlow.title.filterInfo.isFilter = false;
	toolbar.value!.hasFilter = false;
	$table.value?.reinit();
};
const closeAdd = () => {
	toolbar.value!.hasAdd = false;
	$table.value?.reinit();
};
const closeEditor = () => {
	toolbar.value!.hasEdit = false;
	$table.value?.reinit();
};

const closeImport = () => {
	toolbar.value!.hasImport = false;
	$table.value?.reinit();
};

const closeExport = () => {
	toolbar.value!.hasExport = false;
	$table.value?.reinit();
};
const changeAntCol = () => {
	const antCols = $table.value?.getAntCols();
	antCols!.find(c => c.key === 'code')!.title = 'xxx';
};
const changeLanguage = () => {
	dayjs.locale('zh-cn');
	locale.value = zhCN;
};
const toolbarEx = (dataSource: KaTableDataSource) => {
	console.log(dataSource.activeIndex);
};

const $table = ref<InstanceType<typeof KaTable>>();

onBeforeMount(() => {
	console.log('onBeforeMount');
	statusOptions.push(
		...[
			{ label: '开立', value: '0' },
			{ label: '流程中', value: '1' },
			{ label: '结案', value: '2' },
			{ label: '作废', value: '3' },
		]
	);
});

onMounted(() => {});
</script>

<template>
	<ka-table
		v-if="true"
		ref="$table"
		table-title="title"
		size="small"
		:columns="columns"
		:page-size="3"
		:toolbar="{ hasExport: false, hasRemove: false }"
		:locale="locale"
		url="/api"
		:on-before-refresh="
			async () => {
				console.log('onBeforeRefresh');
				return { isSuccess: true, message: 'confirm?' };
			}
		"
		:on-post-load-data="
			async () => {
				console.log('onPostLoadData');
				return { isSuccess: true };
			}
		"
		:on-before-add="
			async () => {
				console.log('onBeforeAdd');
				const editor = $table!.getEditorObj();
				editor.code.display='show';
				return { isSuccess: true, message: 'confirm?' };
			}
		"
		:on-after-add="
			async _data => {
				console.log('onAfterAdd');
				return { isSuccess: true };
			}
		"
		:on-pre-add="
			async (data:any) => {
				console.log('onBeforeInsert',data);
                // await $table?.setEditorVal('remark',['xxx','yyy']);
                return { isSuccess: true };
			}
		"
		:on-before-edit="
			async () => {
				console.log('onBeforeEdit');
				const editor = $table!.getEditorObj();
				editor.code.display='readonly';
				editor.item.display='readonly';
				return { isSuccess: true, message: 'confirm?' };
			}
		"
		:on-after-edit="
			async () => {
				console.log('onAfterEdit');
				return { isSuccess: true };
			}
		"
		:init-filter-conditions="[
			{ key: 'docFlow.modifyDatetime', opt: 'gte', val: dayjs(new Date(2019, 3, 1)), bool: 'and' },
			{ key: 'docFlow.status', opt: 'eq', val: '1', bool: 'and' },
		]"
		:frozen-filter-conditions="[{ key: 'code', opt: 'beg', val: 'QX', bool: 'and' }]"
		:init-sorter-conditions="sorterConditions"
		:is-debug="true"
	>
		<template #toolbar="{ dataSource }">
			<a-space-compact size="small">
				<a-tooltip><a-button type="primary" @click="toolbarEx(dataSource)">x1</a-button></a-tooltip>
			</a-space-compact>
		</template>
		<template #bodyItem="{ column, text }">
			<span v-if="column.key === 'code'">[{{ text }}]</span>
		</template>
	</ka-table>
	<ka-table
		v-if="true"
		table-title="title"
		size="small"
		:columns="columns"
		:page-size="3"
		:toolbar="{ hasExport: false, hasRemove: false }"
		:locale="locale"
		url="/api"
		:is-debug="true"
	>
	</ka-table>
	<a-button @click="testInit">testInit</a-button>
	<a-button @click="closeSorter">closeSorter</a-button>
	<a-button @click="closeFilter">closeFilter</a-button>
	<a-button @click="closeAdd">closeAdd</a-button>
	<a-button @click="closeEditor">closeEditor</a-button>
	<a-button @click="closeImport">closeImport</a-button>
	<a-button @click="closeExport">closeExport</a-button>
	<a-button @click="changeAntCol">changeAntCol</a-button>
	<a-button @click="changeLanguage">localeCN</a-button>
</template>

<style scoped></style>
