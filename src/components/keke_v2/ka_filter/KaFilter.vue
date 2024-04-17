<template>
	<a-flex vertical="vertical" gap="small">
		<a-flex gap="small" class="katable-filter" v-for="(item, i) in items" :key="i">
			<a-select
				class="bool"
				v-model:value="item.bool"
				:show-arrow="false"
				popup-class-name="katable-filter-bool-popup"
				:style="{ visibility: i === 0 ? 'hidden' : undefined }"
				size="nomal"
			>
				<a-select-option value="and">且</a-select-option>
				<a-select-option value="or">或</a-select-option>
			</a-select>
			<ka-filter class="child" :items="item.children" :columns="props.columns" v-if="item.children"></ka-filter>
			<a-space-compact v-else class="flex-auto-width">
				<a-select
					class="col"
					v-model:value="item.key"
					:show-arrow="false"
					:options="colOptions"
					@change="(value:any)=>{onChangeCol(item,value)}"
					size="nomal"
				>
				</a-select>
				<a-select
					class="opt"
					:show-arrow="false"
					v-model:value="item.opt"
					:options="item.optOptions"
					@change="(value:any)=>{onChangeOpt(item,value)}"
					:disabled="!item.key"
					size="nomal"
				>
				</a-select>
				<ka-input
					v-model="item.val"
					:component-type="item.valComponent"
					v-bind="item.valAttrs"
					:value-converter="item.col?.valueConverter"
					:debounce-delay="item.col?.debounceDelay"
					:disabled="!item.opt || item.opt === 'nu' || item.opt === 'nnu'"
					@change="(v:any)=>{onValChange(item,v)}"
					@search="(v:any)=>{onValSearch(item,v)}"
					:options="item.valOptions"
					class="flex-auto-width"
				>
				</ka-input>
			</a-space-compact>
			<a-dropdown size="nomal">
				<template #overlay>
					<a-menu>
						<a-menu-item key="1" @click="addItemDown(i)">
							<SisternodeOutlined />
						</a-menu-item>
						<a-menu-item key="2" @click="addItemUp(i)" class="katable-filter-action-rvs">
							<SisternodeOutlined />
						</a-menu-item>
						<a-menu-divider />
						<a-menu-item key="3" @click="addItemDownGroup(i)">
							<SisternodeOutlined class="katable-filter-action-grp" />
						</a-menu-item>
						<a-menu-item key="4" @click="addItemUpGroup(i)" class="katable-filter-action-rvs">
							<SisternodeOutlined class="katable-filter-action-grp" />
						</a-menu-item>
						<a-menu-divider />
						<a-menu-item :disabled="i === 0 && items.length === 1" key="5" @click="deleteItem(i)">
							<CloseOutlined />
						</a-menu-item>
					</a-menu>
				</template>
				<a-button size="nomal">
					<SettingOutlined />
				</a-button>
			</a-dropdown>
		</a-flex>
	</a-flex>
</template>

<script setup lang="ts">
import { PropType, onBeforeMount, ref } from 'vue';
import { SettingOutlined, SisternodeOutlined, CloseOutlined } from '@ant-design/icons-vue';
import * as lodash from 'lodash-es';
import { KaFilterCol, KaFilterCondition, KaFilterItem, createEmptyItem } from '.';
import KaInput from '../ka_input/KaInput.vue';
import dayjs, { Dayjs } from 'dayjs';

// const conditions = defineModel<KaFilterCondition[]>({});
// const filterItems = defineModel<KaFilterItem[]>();

// const filterItems = ref<KaFilterCondition[]>([]);

const props = defineProps({
	columns: {
		type: Array as PropType<KaFilterCol[]>,
		required: true,
	},
	items: {
		type: Array as PropType<KaFilterItem[]>,
		default: [createEmptyItem()],
	},
});
const items = ref(props.items);
// const items = ref<KaFilterItem[]>([]);
// const columns = ref<KaFilterCol[]>([]);

let colOptions = [] as { label: string; value: any }[];

const optOptions = [
	{ label: '等于', value: 'eq' },
	{ label: '不等于', value: 'neq' },
	{ label: '大于', value: 'gt' },
	{ label: '大于等于', value: 'gte' },
	{ label: '小于', value: 'lt' },
	{ label: '小于等于', value: 'lte' },
	{ label: '开头于', value: 'beg' },
	{ label: '结束于', value: 'end' },
	{ label: '包含', value: 'like' },
	// { label: '列表', value: 'in' },
	{ label: '空', value: 'nu' },
	{ label: '不为空', value: 'nnu' },
];

const onChangeCol = (item: KaFilterItem, _value: any) => {
	console.log('onChangeCol');
	item.col = props.columns.find(col => col.key === item.key);
	item.opt = undefined;
	item.optOptions = getOptOptions(item);
	item.val = '';
	item.valOptions = undefined;
	item.valComponent = undefined;
	item.valAttrs = undefined;
};

const onChangeOpt = (item: KaFilterItem, opt: KaFilterItem['opt']) => {
	console.log('onChangeOpt');
	if (!opt) return;
	// const col = props.columns!.find(col => col.key === item.key);

	let valSingle = lodash.isArray(item.val) ? item.val[0] : item.val;
	let valMult = lodash.isArray(item.val) ? item.val : item.val == null || item.val == '' ? [] : [item.val];
	if (valSingle instanceof dayjs) {
		valSingle = (valSingle as Dayjs).format(item.col?.attrs?.format || 'YYYY-MM-DD');
		valMult[0] = valSingle;
	}
	// condition.dateFormat = col?.dateFormat;

	if (['beg', 'end'].includes(opt)) {
		item.valComponent = 'input';
		item.val = valSingle;
		item.valOptions = undefined;
		item.valAttrs = undefined;
		return;
	}

	if (['nu', 'nnu'].includes(opt)) {
		item.valComponent = 'input';
		item.val = '';
		item.valOptions = undefined;
		item.valAttrs = undefined;
		return;
	}

	if (['eq', 'neq', 'gt', 'gte', 'lt', 'lte', 'like'].includes(opt)) {
		if (item.col!.options) {
			item.valComponent = 'select';
			item.valOptions = lodash.isArray(item.col?.options) ? item.col?.options : undefined;
			item.valAttrs = item.col?.attrs || {};
			item.val = item.valAttrs?.mode === 'combobox' ? valSingle : valMult;
			onValSearch(item, valSingle);
			return;
		}
	}

	if (item.col!.componentType === 'textarea') {
		item.valComponent = 'input';
	} else {
		item.valComponent = item.col!.componentType;
	}

	if (item.valComponent === 'number') {
		item.val = parseFloat(valSingle) || null;
	} else if (item.valComponent === 'date') {
		item.val = valSingle ? dayjs(valSingle, item.col?.attrs?.format || 'YYYY-MM-DD') : null;
	} else {
		item.val = valSingle;
	}

	item.valAttrs = item.col?.attrs;
	item.valOptions = undefined;
};

const onValChange = (item: KaFilterItem, value: any) => {
	console.log('onValChange', value);
	if (item.col?.valueConverter) {
		item.val = value;
	}
	// if (lodash.isArray(value)) {
	// 	const col = props.columns!.find(col => col.col === condition.col);
	// 	if (col!.type === 'number') {
	// 		const lst = value[value.length - 1];
	// 		if (condition.val[condition.val.length - 1] === lst) {
	// 			condition.val[condition.val.length - 1] = parseFloat(lst);
	// 		}
	// 	}
	// }
};
const onValSearch = async (item: KaFilterItem, key: any) => {
	if (lodash.isFunction(item.col?.options)) {
		item.valOptions = await item.col.options(key);
	}
	// console.log(key);
};

const getOptOptions = (item: KaFilterItem) => {
	if (item.col?.componentType === 'number') {
		return optOptions.filter(opt => ['eq', 'neq', 'gt', 'gte', 'lt', 'lte', 'in', 'nu', 'nnu'].includes(opt.value));
	}
	if (item.col?.componentType === 'date') {
		return optOptions.filter(opt => ['eq', 'neq', 'gt', 'gte', 'lt', 'lte', 'nu', 'nnu'].includes(opt.value));
	}
	if (item.col?.componentType === 'select') {
		if (item.col?.attrs?.mode === 'multiple') {
			return optOptions.filter(opt => ['eq', 'neq', 'like', 'nu', 'nnu'].includes(opt.value));
		}
	}
	return optOptions;
};

const createEmptyItemGroup = () => {
	return {
		key: '',
		val: '',
		bool: 'and',
		children: [createEmptyItem()],
	} as KaFilterItem;
};

const addItemUp = (index: number) => {
	items.value!.splice(index, 0, createEmptyItem());
};
const addItemDown = (index: number) => {
	items.value.splice(index + 1, 0, createEmptyItem());
};
const addItemUpGroup = (index: number) => {
	items.value!.splice(index, 0, createEmptyItemGroup());
};
const addItemDownGroup = (index: number) => {
	items.value!.splice(index + 1, 0, createEmptyItemGroup());
};
const deleteItem = (index: number) => {
	items.value!.splice(index, 1);
};
/** 设置条件 */
const setConditions = (conditions: KaFilterCondition[]) => {
	console.log('setConditions');
	if (conditions!.length === 0) {
		items.value = [createEmptyItem()];
		return;
	}

	conditions![0].bool = 'and';
	const _items = [] as KaFilterItem[];
	for (const condition of conditions) {
		_items.push(createItem(condition));
	}

	items.value = _items;
};
const createItem = (condition: KaFilterCondition) => {
	const item = { ...condition } as KaFilterItem;
	if (condition.children) {
		item.children = [];
		for (const child of condition.children) {
			item.children.push(createItem(child));
		}
	} else {
		item.col = props.columns.find(c => c.key === item.key);
		item.optOptions = getOptOptions(item);
		onChangeOpt(item, item.opt);
	}
	return item;
};
/** 获取条件 */
const getConditions = () => {
	return createConditions(items.value!);
};
const createConditions = (items: KaFilterItem[]) => {
	const conditions: KaFilterCondition[] = [];
	for (const item of items) {
		const condition = lodash.cloneDeep(lodash.pick(item, ['key', 'opt', 'val', 'bool'])) as KaFilterCondition;
		if (item.children) {
			condition.children = createConditions(item.children);
		}
		conditions.push(condition);
	}
	return conditions;
};

const render = (columns?:KaFilterCol[]) => {
	// filterItems.value = [createEmptyItem()];
	// props.items = [createEmptyItem()];
	if(columns){
		colOptions = columns.map(c => ({ label: c.title, value: c.key }));
	}else{
		colOptions = props.columns.map(c => ({ label: c.title, value: c.key }));
	}
};

defineExpose({ setConditions, getConditions, render });

onBeforeMount(() => {
	render();
});
</script>

<style scoped>
.bool {
	width: 50px;
	text-align: center;
}
.opt {
	width: 90px;
	text-align: center;
}
.col {
	width: 150px;
}
.w100 {
	width: 100%;
}
.flex-auto-width {
	flex: 1;
}
.child {
	border: 1px dashed #ccc;
	padding: 8px;
}
</style>
<style>
.katable-filter-bool-popup {
	text-align: center;
}
.katable-filter-action-grp {
	border: 2px dashed #ccc;
}
.katable-filter-action-rvs {
	transform: scaleY(-1);
}
</style>
