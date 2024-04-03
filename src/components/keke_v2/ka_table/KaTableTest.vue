<script setup lang="ts">
import { reactive, ref } from 'vue';
import dayjs from 'dayjs';
import { KaTableCols, KaTableDataSource, KaTablePropsToolbar } from '.';
import KaTable from './KaTable.vue';
import { KaSorterCondition } from '../ka_sorter';

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
	tempNum:{
		title: '临时编号',
		editorInfo: {
		    index: 7,
			isPost:false,
		}
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
				options: async () => [
					{ label: '开立', value: '0' },
					{ label: '流程中', value: '1' },
					{ label: '结案', value: '2' },
					{ label: '作废', value: '3' },
				],
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
	{ key: 'docFlow.modifyDatetime', order: null, index: null },
]);

const toolbar = ref<KaTablePropsToolbar | undefined>({
	hasFilter: true,
});

const testInit = () => {
	toolbar.value = {
		hasRemove: false,
	};
	columns.code.listInfo!.title = '公文号码';
	$table.value?.reinit();
};
const toolbarEx = (dataSource: KaTableDataSource) => {
	console.log(dataSource.activeIndex);
};

const $table = ref<InstanceType<typeof KaTable>>();
</script>

<template>
	<ka-table
		v-if="true"
		ref="$table"
		table-title="title"
		size="small"
		:columns="columns"
		:page-size="3"
		:toolbar="toolbar"
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
	<a-button @click="testInit">testInit</a-button>
</template>

<style scoped></style>
