<template>
	<ka-editor ref="$editor" :items-obj="itemsObj"></ka-editor>
	<button @click="onClick">click</button>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { KaEditorItem } from '.';
import KaEditor from './KaEditor.vue';

const $editor = ref<InstanceType<typeof KaEditor>>();

const editorVals = ref({});

const itemsObj = {
	code: {
		key: 'code',
		index: 1.01,
		title: '公文编号',
		width: '100px',
		rules: [{ required: true, message: '请输入公文号' }],
		componentType: 'input',
		position: 'line',
		isPost: true,
		display: 'show',

		// valueConverter?: (value: any) => Promise<any>;
		// customComponent?: Component;
		// debounceDelay?: number;
	},
	'docFlow.title': {
		key: 'docFlow.title',
		index: 1.02,
		title: '标题',
		width: '200px',
		position: 'cling',
		componentType: 'input',
		isPost: true,
		display: 'show',
	},
	item: {
		key: 'item',
		index: 1.11,
		width: '100%',
		componentType: 'number',
		title: '数量',
		position: 'inline',
		attrs: { precision: 0 },
		isPost: true,
		display: 'show',
	},
	deptNo: {
		key: 'deptNo',
		index: 2,
		title: '部门编号',
		width: '100%',
		componentType: 'input',
		position: 'line',
		isPost: true,
		display: 'show',
	},
	remark: {
		key: 'remark',
		index: 5,
		title: '备注',
		width: '500px',
		componentType: 'textarea',
		position: 'line',
		isPost: true,
		display: 'show',
	},
	'docFlow.status': {
		key: 'docFlow.status',
		index: 3,
		title: '状态',
		width: '100%',
		componentType: 'select',
		selectSplit: ';',
		position: 'line',
		isPost: true,
		display: 'show',
		attrs: {
			// mode:'combobox',
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
		onAfterChange: async (value: any, option?: any) => {
			// data.code = 'xxx';
			// $table.value!.validateFields([['code']]);
			console.log($editor.value?.getEditorVal());
			$editor.value?.setEditorVal('code', null);
			console.log('onAfterChange', value, option);
			// return {isSuccess:true,message:'xxx'};
		},
	},
	'docFlow.modifyDatetime': {
		key: 'docFlow.modifyDatetime',
		index: 4,
		title: '修改时间',
		width: '100%',
		componentType: 'date',
		position: 'line',
		isPost: true,
		display: 'show',
		attrs: {
			format: 'YYYY-MM-DD HH:mm:ss',
			showNow: true,
			showToday: true,
			showTime: { format: 'HH:mm' },
		},
	},
} as { [key: string]: KaEditorItem };

const onClick = () => {
	console.log(editorVals.value);
};

onMounted(() => {
	// $editor.value?.reRender();
});
</script>

<style scoped></style>
