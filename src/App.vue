<script setup lang="ts">
import KaTable from './components/keke/katable/KaTable.vue';
import { reactive, ref } from 'vue';
import { KaTableCols, KaTableCol, KaTableDataSource, KaTableRowRecord } from './components/keke/katable';
import KaDragList from './components/keke/kadrag_list/KaDragList.vue';
import KaTableFilter from './components/keke/katable_filter/KaTableFilter.vue';
import KaTableInput from './components/keke/katable_input/KaTableInput.vue';
import dayjs from 'dayjs';
import { KaTableFilterCol } from './components/keke/katable_filter';

const columns = reactive<KaTableCols>({
	code: {
		title: '公文号',
		dbInfo: {
			isPk: true,
		},
		listInfo: {
			index: 0,
		},
		sortInfo: {
			index: 0,
			order: 'ascend',
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
	},
	item: {
		title: '序号',
		dbInfo: {
			isPk: true,
			dataType:'number'
		},
		listInfo: {
			index: 1,
		},
		sortInfo: {
			index: 1,
			order: 'descend',
		},
		editorInfo: {
			index: 2,
			width: '150px',
			position: 'inline',
			rules: [
				{
					validator: async (_rule, _value) => {
						$table.value!.validateFields([['code']]);
						return Promise.resolve();
					},
				},
			],
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
	},
	remark: {
		title: '备注',
		listInfo: {
			index: 5,
		},
		editorInfo: {
			index: 6,
			inputType: 'select',
			selectMode: 'multiple',
			options: [
				{ label: 'xx1', value: 'xx1' },
				{ label: 'xx2', value: 'xx2' },
				{ label: 'xx3', value: 'xx3' },
				{ label: 'xx4', value: 'xx4' },
			],
		},
	},
	docFlow: {
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
				inputType: 'select',
				options:async ()=> [
					{ label: '开立', value: '0' },
					{ label: '流程中', value: '1' },
					{ label: '结案', value: '2' },
					{ label: '作废', value: '3' },
				],
				onAfterChange: async (data: any, value: any, option?: any) => {
					data.code = 'xxx';
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
				order: 'descend',
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

const sorterConditions = ref([
	{ col: 'code', order: 'ascend', title: '公文号' },
	{ col: 'item', order: null, title: '序号' },
	{ col: 'deptNo', order: null, title: '部门编号' },
]);

const test = () => {
	columns.code.title = '公文编号';
	(columns.code as KaTableCol).listInfo.customHeaderCell = () => ({ style: { color: 'red' } });
};
const toolbarEx = (dataSource: KaTableDataSource) => {
	console.log(dataSource.activeIndex);
};

const dragData = ref([
	{ title: '公文号', col: 'code' },
	{ title: '序号', col: 'item' },
]);

const $input = ref();
const inputValue = ref();
const inputOptions = ref([{ value: 'st' }, { value: 'dd' }]);
const onInputSearch = (key: string) => {
	console.log(key);
	inputOptions.value = inputOptions.value.filter(item => item.value.includes(key));
};
const onClickInput = () => {
	inputValue.value = 'd';
	$input.value.resetOption('d');
};

const filterBlocks = ref([
	{ bool: 'and', col: 'code', opt: 'eq', val: 'x1' },
	{ bool: 'and', col: 'code', opt: 'in', val: ['x1'] },
]);
const filterColumns = ref<KaTableFilterCol[]>([
	{ col: 'code', title: '公文号', valOptions: [{ value: 'x1' }, { value: 'x2' }] },
	{ col: 'item', title: '序号', type: 'number', colLength: 1.1 },
]);

const $table = ref<InstanceType<typeof KaTable>>();
</script>

<template>
	<!-- <form-tsx></form-tsx> -->
	<ka-table
		v-if="true"
		ref="$table"
		table-title="title"
		size="small"
		:columns="columns"
		:page-size="3"
		:toolbar="{ hasFilter: true,}"
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
			async data => {
				console.log('onAfterAdd');
				(data.editRecord.docFlow as KaTableRowRecord).status = '1';
				(data.editRecord.docFlow as KaTableRowRecord).modifyDatetime = dayjs();
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
			{ col: 'docFlow.modifyDatetime', opt: 'gte', val: dayjs(new Date(2019, 3, 1)), bool: 'and' },
			{ col: 'docFlow.status', opt: 'eq', val: '1', bool: 'and' },
		]"
		:frozen-filter-conditions="[{ col: 'code', opt: 'beg', val: 'Q', bool: 'and' }]"
		:is-debug="true"
	>
		<template #toolbar="{ dataSource }">
			<a-space-compact size="small">
				<a-tooltip><a-button type="primary" @click="toolbarEx(dataSource)">x1</a-button></a-tooltip>
			</a-space-compact>
		</template>
	</ka-table>
	<a-button @click="test">test</a-button>
	<!-- <ka-table-sorter
		v-model="sorterConditions"
		:columns="[
			{ label: '公文号', value: 'code' },
			{ label: '序号', value: 'item' },
		]"
	></ka-table-sorter> -->

	<!-- <ka-drag-list v-model="dragData">
		<template #renderItem="{ item }">【{{ item.title }}】</template>
		<template #renderAction="{ item }"
			><a>edit</a><a>edit</a><a>edit</a><a>edit</a><a>edit</a><a>edit</a><a>edit</a><a>edit</a></template
		>
	</ka-drag-list> -->
	<!-- <ka-table-filter v-model="filterBlocks" :columns="filterColumns"></ka-table-filter> -->
	<!-- <ka-table-input ref="$input" type="date" :options="inputOptions" @search="onInputSearch" date-format="YYYY-MM-DD" v-model="inputValue"></ka-table-input>
	{{ inputValue }} -->
	<a-date-picker v-model:value="inputValue" :show-now="true"></a-date-picker>
	<!-- <a-button @click="onClickInput">{{ inputValue }}</a-button> -->
</template>

<style scoped></style>
