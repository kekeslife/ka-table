<template>
	<a-form ref="$form" :model="editorVals" layout="vertical">
		<div
			class="editor-row"
			v-for="(editorRow, rowIndex) in editorRows"
			:key="rowIndex"
			:style="{ display: lodash.flattenDepth(editorRow, 2).some(l => l.display !== 'hide') ? 'flex' : 'none' }"
		>
			<a-form-item
				v-for="(editorGroup, groupIndex) in editorRow"
				:key="groupIndex"
				:label="editorGroup[0].title"
				:rules="editorGroup[0].rules"
				:name="editorGroup[0].key"
				:class="{ hide: editorGroup[0].display === 'hide' }"
				class="item-flex"
			>
				<a-input-group :compact="editorGroup.length > 1">
					<template v-for="(editorItem, itemIndex) in editorGroup" :key="itemIndex">
						<ka-input
							v-if="itemIndex === 0"
							:style="{ width: editorItem.width, display: editorItem.display === 'hide' ? 'none' : 'auto' }"
							v-model="editorVals[editorItem.key!]"
							v-bind="editorItem.attrs"
							:component-type="editorItem.componentType"
							:value-converter="editorItem.valueConverter"
							:custom-component="editorItem.customComponent"
							:debounce-delay="editorItem.debounceDelay"
							:disabled="editorItem.display === 'readonly'"
							@change="(val:any,option:any)=>{onValChange(val, editorItem,option)}"
							@search="(key:any)=>{onSearch(key, editorItem)}"
							size="normal"
						></ka-input>
						<a-form-item-rest v-else>
							<ka-input
								:style="{ width: editorItem.width, display: editorItem.display === 'hide' ? 'none' : 'auto' }"
								v-model="editorVals[editorItem.key!]"
								v-bind="editorItem.attrs"
								:component-type="editorItem.componentType"
								:value-converter="editorItem.valueConverter"
								:custom-component="editorItem.customComponent"
								:debounce-delay="editorItem.debounceDelay"
								:disabled="editorItem.display === 'readonly'"
								@change="(val:any,option:any)=>{onValChange(val, editorItem,option)}"
								@search="(key:any)=>{onSearch(key, editorItem)}"
								size="normal"
							></ka-input>
						</a-form-item-rest>
					</template>
				</a-input-group>
			</a-form-item>
		</div>
	</a-form>
</template>

<script setup lang="ts">
import { onBeforeMount, ref } from 'vue';
import { KaEditorItem, KaEditorRow, render } from '.';
import KaInput from '../ka_input/KaInput.vue';
import * as lodash from 'lodash-es';
import { FormInstance } from 'ant-design-vue';
import { NamePath, ValidateOptions } from 'ant-design-vue/es/form/interface';

// const editorVals = defineModel({ type: Object as PropType<{ [key: string]: any }>, default: {}, required: true });

const props = defineProps({
	itemsObj: { type: Object, required: true }
});

const $form = ref<FormInstance>(null as unknown as FormInstance);
// [ [ item1, item2 ], [ item3 ] ]
const editorRows = ref<KaEditorRow[]>([]);
const editorVals = ref<{ [key: string]: any }>({});

const reRender = () => {
	editorRows.value = render(props.itemsObj);
	// console.log(editorRows.value);
};

const onValChange = async (val: any, col: KaEditorItem, option?: any) => {
	if (col.onAfterChange) {
		await col.onAfterChange(val, option);
	}
	// console.log(val, editorVals.value);
};
const onSearch = async (key: any, col: KaEditorItem) => {
	if (lodash.isFunction(col.options)) {
		if (col.attrs) {
			col.attrs.options = await col.options(key);
		}
	}
	// console.log(key);
};
const getEditorVal = (key?: string) => {
	return key ? editorVals.value[key] : editorVals.value;
};
const setEditorVal = async (key: string, val: any, trigChange: boolean = true, trigSearch: boolean = false) => {
	if (key) {
		editorVals.value[key] = val;
		trigSearch && (await onSearch(val, props.itemsObj[key]));
		trigChange && (await onValChange(val, props.itemsObj[key]));
		try {
			await $form.value.validateFields(key);
		} catch {}
	}
};
/** 清空编辑框值 */
const clearEditor = () => {
	for (const key in editorVals.value) {
		editorVals.value[key] = null;
	}
	$form.value.resetFields();
};

// #region 校验

const resetFields = (name?: NamePath | undefined) => $form.value.resetFields(name);
const clearValidate = (name?: NamePath | undefined) => $form.value.clearValidate(name);
const validate = (nameList?: string | NamePath[] | undefined, options?: ValidateOptions | undefined) =>
	$form.value.validate(nameList, options);
const validateFields = (nameList?: string | NamePath[] | undefined, options?: ValidateOptions | undefined) =>
	$form.value.validateFields(nameList, options);
const scrollToField = (name: NamePath, options?: {} | undefined) => $form.value.scrollToField(name, options);
// #endregion 校验


defineExpose({
	reRender,
	getEditorVal,
	setEditorVal,
	clearEditor,
	resetFields,
	clearValidate,
	validate,
	validateFields,
	scrollToField,
});
onBeforeMount(() => {
	editorRows.value = render(props.itemsObj);
});
</script>

<style scoped>
.editor-row {
	display: flex;
	flex-direction: row;
	gap: 1rem;
}
.editor-row :deep(.ant-form-row){
	display: block;
}
/* .item-flex {
	flex-grow: 1;
} */
.hide {
	display: none;
}
</style>
