<template>
	<a-input-number
		style="width: 100%"
		v-if="props.type === 'number'"
		v-bind="$attrs"
		v-model:value="value"
		:precision="precision"
		@change="onChange"
		allow-clear
	>
	</a-input-number>
	<a-date-picker
		v-else-if="props.type === 'date'"
		v-model:value="value"
		bind="$attrs"
		style="width: 100%"
		@change="onChange"
		:show-time="showTime"
		:format="props.dateFormat || 'YYYY/MM/DD'"
		:show-now="true"
		:show-today="true"
	></a-date-picker>
	<a-select
		v-else-if="props.type === 'select'"
		v-bind="$attrs"
		v-model:value="value"
		style="width: 100%"
		@search="onSearch"
		@change="onChange"
		:show-search="true"
		:options="props.options"
		:mode="props.selectMode"
		allow-clear
	></a-select>
	<a-textarea
		v-else-if="props.type === 'textarea'"
		v-bind="$attrs"
		v-model:value="value"
		style="width: 100%"
		allow-clear
		@change="onChange"
	>
	</a-textarea>
	<a-input v-else @change="onChange" v-bind="$attrs" v-model:value="value" style="width: 100%" allow-clear> </a-input>
</template>

<script setup lang="ts">
import { PropType, computed, ref, watch } from 'vue';
import debounce from 'lodash/debounce';
import lodash from 'lodash';

const value = defineModel();
const emit = defineEmits<{
	change: [value: any, option: any];
	search: [value: any];
}>();

const props = defineProps({
	type: { type: String as PropType<'number' | 'date' | 'select' | 'textarea' | 'string'> },
	options: { type: Array as PropType<{ label?: string; value: any }[]> },
	colLength: { type: Number },
	dateFormat: { type: String },
	selectMode: { type: String as PropType<'multiple' | 'tags' | 'combobox'>, default: 'combobox' },
});
const showTime = ref<boolean | { format: string }>(false);

const resetOption = (key: string) => {
	if (props.type === 'select') {
		onSearch(key || (value.value as string));
	}
	onChange(value.value);
};

defineExpose({ resetOption });
watch(
	() => props.dateFormat,
	() => {
		// console.log('watch dateFormat');
		if (!props.dateFormat) {
			showTime.value = false;
			return;
		}

		const arr = [] as number[];
		const hMatch = props.dateFormat.match(/h+/i);
		if (hMatch) {
			arr.push(hMatch.index!);
			arr.push(hMatch.index! + hMatch[0].length);
		}
		const mHatch = props.dateFormat.match(/m+/);
		if (mHatch) {
			arr.push(mHatch.index!);
			arr.push(mHatch.index! + mHatch[0].length);
		}
		const sHatch = props.dateFormat.match(/s+/);
		if (sHatch) {
			arr.push(sHatch.index!);
			arr.push(sHatch.index! + sHatch[0].length);
		}

		if (arr.length === 0) {
			showTime.value = false;
			return;
		}

		const startIndex = Math.min(...arr);
		const endIndex = Math.max(...arr);
		showTime.value = { format: props.dateFormat.substring(startIndex, endIndex + 1) };
	},
	{
		immediate: true,
	}
);

const precision = computed(() => {
	if (props.colLength === undefined) return undefined;

	const arr = props.colLength.toString().split('.');
	if (arr.length === 1) return 0;
	return parseInt(arr[1]);
});

const onSearchDebounce = debounce((searchKey: string) => {
	// console.log('onSearchDebounce');
	if (!searchKey) return;
	emit('search', searchKey);
}, 300);
const onChangeDebounce = debounce(async (value: any, option: any) => {
	trigChange(value, option);
}, 300);
const trigChange = (value: any, option: any) => {
	// console.log('trigChange');
	emit('change', value, option);
};
const onChange = (v: any, option?: any) => {
	if(v instanceof Event) {
		v = (v.target as HTMLInputElement).value;
	}
	if (!['date', 'select'].includes(props.type!)) {
		onChangeDebounce(v, option);
	} else {
		trigChange(v, option);
	}
};
const onSearch = (searchKey: string) => {
	onSearchDebounce(searchKey);
};

// console.log(props.type)
</script>

<style scoped></style>
