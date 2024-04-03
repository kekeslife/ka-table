<template>
	<ka-filter ref="$filter" :columns="filterCols"></ka-filter>
    <a-button @click="setConditions">setConditions</a-button>
    <a-button @click="getConditions">getConditions</a-button>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { KaFilterCol, KaFilterCondition } from '.';
import KaFilter from './KaFilter.vue';
import dayjs from 'dayjs';
import lodash from 'lodash';

const filterCols: KaFilterCol[] = [
	{
		key: 'code',
		title: '公文编号',
		componentType: 'input',
	},
	{
		key: 'docFlow.title',
		title: '标题',
		componentType: 'input',
		valueConverter: async val => {
			return val + 'x';
		},
	},
	{
		key: 'item',
		componentType: 'number',
		title: '数量',
		attrs: { precision: 0 },
	},
	{
		key: 'docFlow.modifyDatetime',
		componentType: 'date',
		title: '修改日期',
		attrs: {
			format: 'YYYY-MM-DD HH:mm',
			showNow: true,
			showToday: true,
			showTime: { format: 'HH:mm' },
		},
	},
	{
		key: 'docFlow.status',
		title: '状态',
		componentType: 'select',
		attrs: {
			mode:'combobox',
			showSearch: true,
			options: [
				{ label: '开立', value: '0' },
				{ label: '流程中', value: '1' },
				{ label: '结案', value: '2' },
				{ label: '作废', value: '3' },
			],
		},
		options: async () => [
			{ label: '开立', value: '0' },
			{ label: '流程中', value: '1' },
			{ label: '结案', value: '2' },
		],
	},
	{
		key: 'remark',
		title: '备注',
		componentType: 'select',
		attrs: {
			mode: 'multiple',
			showSearch: true,
			options: [
				{ label: '开立', value: '0' },
				{ label: '流程中', value: '1' },
				{ label: '结案', value: '2' },
				{ label: '作废', value: '3' },
			],
		},
		options: async (x: string) => [
			{ label: `${x}-1`, value: `${x}-1` },
			{ label: `${x}-2`, value: `${x}-2` },
		],
	},
];

const conditions: KaFilterCondition[] = [
	{
		key: 'docFlow.modifyDatetime',
		opt: 'gte',
		val: dayjs(new Date(2019, 3, 1)),
		bool: 'and',
	},
	{ key: 'docFlow.status', opt: 'eq', val: '1', bool: 'and' },
];

const $filter = ref<InstanceType<typeof KaFilter>>();

const setConditions = ()=>{
    $filter.value?.setConditions(conditions);
}
const getConditions = ()=>{
    const result = $filter.value?.getConditions();
    if(lodash.eq(result,conditions)){
        console.log(true);
    }else{
        console.error(result);
    }
}
</script>

<style scoped></style>
