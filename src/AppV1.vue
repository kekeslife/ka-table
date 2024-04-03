<script setup lang="ts">
// import { ref } from 'vue';
// import HelloWorld from './components/HelloWorld.vue'
// import {LeftCircleFilled} from '@ant-design/icons-vue'
import { Ref, ref } from 'vue';
import KaTable from './components/keke_v1/KaTable.vue';
import { KaTableCols, KaTableColsType, KaTableRecord } from './components/keke/type';
import dayjs from 'dayjs';
import { Rule } from 'ant-design-vue/es/form';

const columns: Ref<KaTableCols<any>> = ref([
	{
		title: '公文号',
		dataIndex: 'code',
		isRequired: true,
		dbInfo: { isPk: true },
		//// sorter: {multiple:4},
		//// defaultSortOrder:'ascend',
		initSorter: { index: 1, order: 'ascend' },
		isFilt: true,
		filteredValue: ['Q02'],
		//// filters:[{text:'xx',value:'xx'}],
		//// filterSearch: true,
		//// onFilter: (value, record) => record.name.startsWith(value),
		listInfo: {
			index: 0,
		},
		editInfo: {
			index: 1,
			width: '150px',
			rules: [{ required: true, message: '请输入公文号' }],
			display:'readonly'
		},
	},
	{
		title: '序号',
		dataIndex: 'item',
		isRequired: true,
		dbInfo: { isPk: true, colType: 'number' },
		initSorter: { index: 2, order: 'ascend' },
		isFilt: true,
		listInfo: {
			index: 2,
		},
		editInfo: {
			index: 2,
			width: '150px',
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
	{
		title: '部门代码',
		dataIndex: 'deptNo',
		isRequired: true,
		isFilt: true,
		listInfo: {
			index: 3,
		},
		editInfo: {
			index: 3,
			width: '150px',
		},
	},
	{
		title: '标题',
		dataIndex: ['docFlow', 'title'],
		isFilt: true,
		listInfo: {
			index: 1.1,
		},
		editInfo: {
			index: 1.1,
			width: '150px',
		},
	},
	{
		title: '状态',
		dataIndex: ['docFlow', 'status'],
		isFilt: true,
		listInfo: {
			index: 4,
		},
		editInfo: {
			index: 4,
			width: '150px',
			inputType: 'select',
			options: [
				{ label: '开立', value: '0' },
				{ label: '流程中', value: '1' },
				{ label: '结案', value: '2' },
				{ label: '作废', value: '3' },
			],
			onAfterChange: async (data: any) => {
				console.log('onAfterChange');
				return { isSuccess: true };
			},
		},
	},
	{
		title: '建立日期',
		dataIndex: ['docFlow', 'createDatetime'],
		dbInfo: { colType: 'date', isPost: false },
		listInfo: {
			index: 0,
		},
		editInfo: {
			index: 1,
			position: 'inline',
			width: '150px',
			// display:'readonly',
		},
	},
	// {
	// 	title: '住址',
	// 	dataIndex: 'address',
	// 	key: 'address',
	// 	isFilt: true,
	// 	// initSorter:{index:2,order:'descend'},
	// 	isSort: true,
	// 	editInfo: {
	// 		index: 2,
	// 		// position:'inline',
	// 		width: '150px',
	// 		inputType: 'select',
	// 		// options:[{text:'xx1',value:'xx1'},{text:'xx2',value:'xx2'}],
	// 		options: async (data: any, key: string) => {
	// 			console.log(JSON.stringify(data.editRecord), key);
	// 			return [
	// 				{ label: 'xx1', value: 'xx1' },
	// 				{ label: 'xx2', value: 'xx2' },
	// 			];
	// 		},
	// 		onAfterChange: async (data: KaTableRecord) => {
	// 			data.editRecord.address = 'onAfterChange';
	// 			data.editRecord.age = 'age';
	// 		},
	// 	},
	// 	// attrs:{
	// 	//   status:'error'
	// 	// }
	// 	// sorter: {multiple:0},
	// 	// defaultSortOrder:'descend',
	// },
]);

const title = ref('ka-table');
const tooltipOpen = ref(false);
const $table = ref<InstanceType<typeof KaTable>>();

// const h = ref(null);
const test = () => {
	// title.value = 'xx';
	//tooltipOpen.value = true;
	// console.log(columns.value);

	columns.value[0].title = '公文编号';
	columns.value[0].editInfo!.display = 'readonly';
	$table.value!.resetFields();
};

function tooltipContainer() {
	return document.getElementById('s');
}
</script>

<template>
	<!-- <HelloWorld ref="h" msg="Click Me" type="default">
    <template #icon>
          <LeftCircleFilled />
        </template>
  </HelloWorld> -->
	<div style="padding: 1rem">
		<ka-table
			ref="$table"
			:table-title="title"
			size="small"
			theme="#ffb514"
			:columns="columns"
			:page-size="3"
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
					data.editRecord.docFlow.status = '1';
					data.editRecord.docFlow.createDatetime = dayjs();
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
			:init-where-conditions="[{ col: 'docFlow.createDatetime', opt: 'gte', val: new Date(2019, 1, 1), bool: 'and' }]"
			:is-debug="true"
		>
			<template #toolbar="{ col1 }">{{ col1 }}</template>
			<template #bodyItem="{ text, index, record, column }">
				<template v-if="column.key === 'age'">{{ text }}岁</template>
			</template>
		</ka-table>
	</div>

	<span id="s" style="margin-left: 100px" @click="test">xxx</span>

	<a-tooltip
		v-model:open="tooltipOpen"
		:getPopupContainer="tooltipContainer"
		:auto-adjust-overflow="true"
		placement="right"
	>
		<template #title>ssssss</template>
	</a-tooltip>
</template>

<style scoped></style>
./components/keke_v1/KaTable.vue./components/keke_v1/type