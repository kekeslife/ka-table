<template>
	<a-input-number
		style="width: 100%"
		v-if="props.column?.editInfo?.inputType === 'number'"
		v-bind="$attrs"
		v-model:value="value"
		:precision="precision"
	>
	</a-input-number>
	<a-date-picker
		v-else-if="props.column?.editInfo?.inputType === 'date'"
		v-bind="$attrs"
		v-model:value="value"
		style="width: 100%"
	></a-date-picker>
	<a-select
		v-else-if="props.column?.editInfo?.inputType === 'select'"
		v-bind="$attrs"
		v-model:value="value"
		style="width: 100%"
		@search="onSearch"
		@change="onChange"
		:show-search="true"
		:options="options"
	></a-select>
	<a-textarea
		v-else-if="props.column?.editInfo?.inputType === 'textarea'"
		v-bind="$attrs"
		v-model:value="value"
		style="width: 100%"
	>
	</a-textarea>
	<a-input v-else v-bind="$attrs" v-model:value="value" style="width: 100%"> </a-input>
</template>

<script setup lang="ts">
import { PropType, Ref, computed, ref, watch } from 'vue';
import { KaTableCol, KaTableOptionItem, KaTableRecord, isArray, isFunction } from './type';
import debounce from 'lodash/debounce';

const value = defineModel();

// const props = defineProps<{
// 	inputType?: KaTableInputType;
// 	options?: KaTableOptionItem[] | ((record: KaTableRecord, key: string) => Promise<KaTableOptionItem[]>);
// 	colLength?: number;
// 	dataSource: KaTableRecord;
// }>();

const props = defineProps({
	column: { type: Object as PropType<KaTableCol<any>>, isRequired: true },
	dataSource: { type: Object as PropType<KaTableRecord>, isRequired: true },
	manualResetOption: { type: Boolean },
});

watch(
	() => props.manualResetOption,
	_ => {
		if (props.column?.editInfo?.inputType === 'select') {
			onSearch(value.value as string);
		}
		onChange();
		// onSearch(value[0])
	}
);

const options: Ref<KaTableOptionItem[]> = ref([]);
let _optionHandle: ((record: KaTableRecord, key: string) => Promise<KaTableOptionItem[]>) | undefined;

if (isFunction(props.column?.editInfo?.options)) {
	_optionHandle = props.column?.editInfo?.options as (
		record: KaTableRecord,
		key: string
	) => Promise<KaTableOptionItem[]>;
} else if (isArray(props.column?.editInfo?.options)) {
	options.value = props.column?.editInfo?.options as KaTableOptionItem[];
}

const precision = computed(() => {
	if (props.column?.dbInfo?.colLength === undefined) return undefined;

	const arr = props.column.dbInfo.colLength.toString().split('.');
	if (arr.length === 1) return 0;
	return parseInt(arr[1]);
});

const onSearchDebounce = debounce(async (searchKey: string) => {
	if (!searchKey) return;
	if (_optionHandle !== undefined) {
		options.value = await _optionHandle(props.dataSource!, searchKey);
	}
}, 300);
const onChange = async () => {
	if (props.column?.editInfo?.onAfterChange !== undefined) {
		await props.column?.editInfo?.onAfterChange(props.dataSource!);
	}
};
const onSearch = (searchKey: string) => {
	onSearchDebounce(searchKey);
};
</script>

<style scoped></style>
