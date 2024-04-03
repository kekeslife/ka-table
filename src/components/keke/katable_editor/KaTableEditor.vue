<template>
	<a-form ref="$form" :model="editTempRecord" layout="vertical">
		<!-- colRow [ [col1, col2] ], [ [col1], [col2] ] -->
		<template v-for="(colRow, _index) in editorInfo" :key="_index">
			<!-- [ [col1] ] line -->
			<template v-if="colRow.length === 1 && colRow[0].length === 1">
				<a-form-item
					:label="colRow[0][0].editorInfo!.title"
					:style="{ width: colRow[0][0].editorInfo!.width,display: colRow[0][0].editorInfo!.display==='hide' }"
					:rules="colRow[0][0].editorInfo!.rules"
					:name="colRow[0][0].key"
				>
					<ka-input
						v-model="editTempRecord[colRow[0][0].key!]"
						v-bind="colRow[0][0].editorInfo!.attrs"
						:col-length="colRow[0][0].dbInfo?.colLength"
						:type="colRow[0][0].editorInfo?.inputType"
						:disabled="colRow[0][0].editorInfo?.display === 'readonly'"
						:options="editOptions[colRow[0][0].key!]"
						:date-format="colRow[0][0].dbInfo?.dateFormat"
						:selectMode="colRow[0][0].editorInfo?.selectMode"
						@change="(val:any,option:any)=>{onValChange(val, colRow[0][0],option)}"
						@search="(key:any)=>{onSearch(key, colRow[0][0])}"
						size="normal"
					>
					</ka-input>
				</a-form-item>
			</template>
			<!-- colGroup [col1, col2] 、 [col1], [col2] -->
			<template v-else>
				<div class="edit-item-inline-block">
					<template v-for="(colGroup, _colIndex) in colRow" :key="_colIndex">
						<!-- [col1] inline -->
						<template v-if="colGroup.length === 1">
							<a-form-item
								:label="colGroup[0].editorInfo!.title"
								:style="{ width: colGroup[0].editorInfo!.width,display: colGroup[0].editorInfo!.display==='hide'}"
								:rules="colGroup[0].editorInfo!.rules"
								:name="colGroup[0].key"
							>
								<ka-input
									v-model="editTempRecord[colGroup[0].key!]"
									v-bind="colGroup[0].editorInfo!.attrs"
									:col-length="colGroup[0].dbInfo?.colLength"
									:type="colGroup[0].editorInfo?.inputType"
									:disabled="colGroup[0].editorInfo?.display === 'readonly'"
									:options="editOptions[colGroup[0].key!]"
									:date-format="colGroup[0].dbInfo?.dateFormat"
									:selectMode="colGroup[0].editorInfo?.selectMode"
									@change="(val:any,option:any)=>{onValChange(val, colGroup[0],option)}"
									@search="(key:any)=>{onSearch(key, colGroup[0])}"
									size="normal"
								>
								</ka-input>
							</a-form-item>
						</template>
						<!-- [col1, col2] cling -->
						<a-form-item
							v-else
							:label="colGroup[0].editorInfo!.title"
							:style="{display: colGroup[0].editorInfo!.display==='hide'}"
							:rules="colGroup[0].editorInfo!.rules"
							:name="colGroup[0].key"
						>
							<a-input-group compact>
								<template v-for="(col, _i) in colGroup" :key="_i">
									<ka-input
										v-if="_i === 0"
										:style="{ width: col.editorInfo!.width, display: col.editorInfo!.display==='hide' }"
										v-model="editTempRecord[col.key!]"
										v-bind="col.editorInfo!.attrs"
										:col-length="col.dbInfo?.colLength"
										:type="col.editorInfo?.inputType"
										:disabled="col.editorInfo?.display === 'readonly'"
										:options="editOptions[col.key!]"
										:date-format="col.dbInfo?.dateFormat"
										:selectMode="col.editorInfo?.selectMode"
										@change="(val:any,option:any)=>{onValChange(val, col,option)}"
										@search="(key:any)=>{onSearch(key, col)}"
										size="normal"
									></ka-input>
									<a-form-item-rest v-else
										><ka-input
											:style="{ width: col.editorInfo!.width, display: col.editorInfo!.display==='hide' }"
											v-model="editTempRecord[col.key!]"
											v-bind="col.editorInfo!.attrs"
											:col-length="col.dbInfo?.colLength"
											:type="col.editorInfo?.inputType"
											:disabled="col.editorInfo?.display === 'readonly'"
											:options="editOptions[col.key!]"
											:date-format="col.dbInfo?.dateFormat"
											:selectMode="col.editorInfo?.selectMode"
											@change="(val:any,option:any)=>{onValChange(val, col,option)}"
											@search="(key:any)=>{onSearch(key, col)}"
											size="normal"
										></ka-input
									></a-form-item-rest>
								</template>
							</a-input-group>
						</a-form-item>
					</template>
				</div>
			</template>
		</template>
	</a-form>
</template>

<script setup lang="ts">
import { PropType, onBeforeMount, reactive, ref, watch } from 'vue';
import { KaTableCol, KaTableCols, KaTableRowRecord } from '../katable';
import KaInput from '../katable_input/KaTableInput.vue';
import lodash from 'lodash';
import { NamePath } from 'ant-design-vue/es/form/interface';
import { FormInstance } from 'ant-design-vue';

const editRecord = defineModel({
	type: Object as PropType<{ [key: string]: any }>,
	required: true,
});

const editTempRecord = reactive<{ [key: string]: any }>({});
const editorInfo = ref<KaTableCol[][][]>();
const editOptions = reactive<{ [key: string]: any }>({});
const keys = [] as string[];

const props = defineProps({
	columns: {
		type: Object as PropType<KaTableCols>,
		required: true,
	},
	// curRecord: {
	// 	type: [Object, null] as PropType<KaTableRowRecord | null>,
	// 	required: true,
	// },
	initHandle: {
		type: String as PropType<'only_search'|'with_search'|''>,
		required: true,
	},
});

const $form = ref<FormInstance>(null as unknown as FormInstance);
defineExpose({
	resetFields: (nameList?: NamePath[]) => {
		$form.value.resetFields(nameList as NamePath);
	},
	clearValidate: (nameList?: NamePath[]) => {
		$form.value.clearValidate(nameList as NamePath);
	},
	validate: (
		nameList?: NamePath[]
	): Promise<{
		[key: string]: any;
	}> => $form.value.validate(nameList),
	validateFields: (
		nameList?: NamePath[]
	): Promise<{
		[key: string]: any;
	}> => $form.value.validateFields(nameList),
	scrollToField: (name: NamePath, options: [ScrollOptions]) => {
		$form.value.scrollToField(name, options);
	},
	initValues: () => {
		initValues();
	},
});

const onValChange = async (val: any, col: KaTableCol, option?: any) => {
	if (props.initHandle==='only_search') {
		onSearch(val, col);
		return;
	}
	console.log(val);
	if (props.initHandle==='with_search') {
		onSearch(val, col);
	}
	lodash.set(editRecord.value, col.key!, val);
	try {
		await $form.value.validateFields([[col.key!]]);
	} catch {}
	if (col.editorInfo?.onAfterChange) {
		await col.editorInfo?.onAfterChange(editRecord.value, val, option);
	}
};
const onSearch = async (key: any, col: KaTableCol) => {
	if (lodash.isFunction(col.editorInfo?.options)) {
		editOptions[col.key!] = await col.editorInfo?.options(editRecord.value, key);
	}
};
const initValues = () => {
	// for (const key of keys) {
	// 	const col = lodash.get(props.columns, key) as KaTableCol;
	// 	const val = lodash.get(editRecord.value, key);
	// 	onValChange(val, col);
	// 	if (col.editorInfo?.inputType === 'select') {
	// 		onSearch(val, col);
	// 	}
	// }
};

onBeforeMount(() => {
	const editCols = [] as KaTableCol[];
	const _editorInfo = [] as KaTableCol[][][];

	const colHandle = (col: KaTableCol | { [key: string]: KaTableCol }) => {
		if (col._katableIsCol) {
			let _c = col as KaTableCol;
			if (_c.editorInfo?.index) {
				editCols.push(_c);
				lodash.set(editRecord.value, _c.key!, ['multiple', 'tags'].includes(_c.editorInfo?.selectMode!) ? [] : null);
				// editRecord.value[_c.key!] = ['multiple', 'tags'].includes(_c.editorInfo?.selectMode!) ? [] : null;
				editOptions![_c.key!] = lodash.isArray(_c.editorInfo?.options) ? _c.editorInfo?.options : [];
				keys.push(_c.key!);
				editTempRecord[_c.key!] = ['multiple', 'tags'].includes(_c.editorInfo?.selectMode!) ? [] : null;
				watch(
					() => lodash.get(editRecord.value, _c.key!),
					v => {
						console.log('watch edit col ', _c.key, v);
						if (v === editTempRecord[_c.key!]) return;
						editTempRecord[_c.key!] = v;
						onValChange(v, _c);
					}
				);
				// watch(
				// 	() => editTempRecord[_c.key!],
				// 	(v,pv) => {
				// 		if(v !== pv){
				// 		console.log('watch temp edit col ' , _c.key , v,pv);
				// 		// setObjectPropVal(m.key, dataSource.editRecord, v);
				// 		lodash.set(editRecord.value, _c.key!, v);
				// 		}
				// 	}
				// );
			}
		} else {
			for (const key in col) {
				const _col = (col as { [key: string]: KaTableCol })[key];
				colHandle(_col);
			}
		}
	};

	for (const colName in props.columns) {
		colHandle(props.columns[colName]);
	}

	// [
	// 	[[col1 inline],[col1,col2 cling],[col3 inline]]
	// ]
	editCols.sort((a, b) => a.editorInfo!.index - b.editorInfo!.index);
	if (editCols.length) {
		let preCol: KaTableCol[][] | null = null;
		for (let col of editCols) {
			if (col.editorInfo!.position === 'line' || preCol === null) {
				preCol = [[col]];
				_editorInfo.push(preCol);
			} else if (col.editorInfo!.position === 'inline') {
				preCol.push([col]);
			} else if (col.editorInfo!.position === 'cling') {
				preCol[preCol.length - 1].push(col);
			}
		}
		editorInfo.value = _editorInfo;
	}
});
</script>

<style scoped></style>
