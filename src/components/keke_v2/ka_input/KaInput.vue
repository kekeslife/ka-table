<template>
	<component
		v-model:value="value"
		:is="props.customComponent || components[componentType]"
		v-bind="$attrs"
		@change="onChange"
		@search="onSearch"
		show-search
		allow-clear
	/>
</template>

<script setup lang="ts">
import { DatePicker, Input, InputNumber, Select, Textarea } from 'ant-design-vue';
import { type Component, PropType, markRaw } from 'vue';
import debounce from 'lodash-es/debounce';

const value = defineModel<any>();

const emit = defineEmits<{
	change: [value: any, option: any];
	search: [value: any];
}>();

const props = defineProps({
	/** 自定义值转换 */
	valueConverter: { type: Function as PropType<(value: any) => Promise<any>> },
	/** 内置输入框类型 */
	componentType: {
		type: String as PropType<'input' | 'textarea' | 'number' | 'date' | 'select'>,
		default: 'input',

		// validator(value, props) {
		// 	if (props.options == null && value == 'select') {
		// 		return false;
		// 	}
		// 	return true;
		// },
	},
	/** 自定义组件 */
	customComponent: {
		type: Object as PropType<Component>,
	},
	/** 延时输出 */
	debounceDelay: { type: Number, default: 300 },
	/** select 选项列表 */
	// options: {
	// 	type: Array as PropType<SelectProps['options']>,
	// },
});

const components = {
	input: markRaw(Input),
	textarea: markRaw(Textarea),
	number: markRaw(InputNumber),
	date: markRaw(DatePicker),
	select: markRaw(Select),
};

const onSearchDebounce = debounce((searchKey: string) => {
	// console.log('onSearchDebounce');
	if (!searchKey) return;
	emit('search', searchKey);
}, props.debounceDelay);

const onChangeDebounce = debounce(async (value: any, option: any) => {
	await trigChange(value, option);
}, props.debounceDelay);

const trigChange = async (value: any, option: any) => {
	// console.log('trigChange');
	if (props.valueConverter) {
		value = await props.valueConverter(value);
	}
	emit('change', value, option);
};

const onChange = async (v: any, option?: any) => {
	if (v instanceof Event) {
		v = (v.target as HTMLInputElement).value;
	}

	if (props.debounceDelay === 0 || ['date', 'select'].includes(props.componentType)) {
		await trigChange(v, option);
		return;
	}

	onChangeDebounce(v, option);
};

const onSearch = (searchKey: string) => {
	onSearchDebounce(searchKey);
};
</script>

<style scoped></style>
