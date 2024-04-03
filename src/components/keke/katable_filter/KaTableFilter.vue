<template>
	<a-flex vertical="vertical" gap="small">
		<a-flex gap="small" class="katable-filter" v-for="(condition, i) in conditions" :key="i">
			<a-select
				class="bool"
				v-model:value="condition.bool"
				:show-arrow="false"
				popup-class-name="katable-filter-bool-popup"
				:style="{ visibility: i === 0 ? 'hidden' : undefined }"
				size="nomal"
			>
				<a-select-option value="and">且</a-select-option>
				<a-select-option value="or">或</a-select-option>
			</a-select>
			<ka-table-filter
				class="child"
				v-model="condition.children"
				:columns="props.columns"
				v-if="condition.children"
			></ka-table-filter>
			<a-space-compact v-else class="flex-auto-width">
				<a-select
					class="col"
					v-model:value="condition.col"
					:show-arrow="false"
					:options="props.columns"
					:field-names="{ label: 'title', value: 'col' }"
					@change="(value:any,option:any)=>{onChangeCol(condition,value,option)}"
					size="nomal"
				>
				</a-select>
				<a-select
					class="opt"
					:show-arrow="false"
					v-model:value="condition.opt"
					:options="condition.options"
					@change="(value:any)=>{onChangeOpt(condition,value)}"
					:disabled="!condition.col"
					size="nomal"
				>
				</a-select>
				<ka-table-input
					:col-length="condition.colLength"
					:type="condition.type"
					v-model="condition.val"
					:mode="condition.type === 'date' ? undefined : condition.multiple ? 'tags' : 'combobox'"
					:disabled="!condition.opt || condition.disabled"
					:options="condition.valOptions"
					:date-format="condition.dateFormat"
					@change="(v:any)=>{onValChange(condition,v)}"
					class="flex-auto-width"
				>
				</ka-table-input>
			</a-space-compact>
			<a-dropdown size="nomal">
				<template #overlay>
					<a-menu>
						<a-menu-item key="1" @click="addConditionDown(i)">
							<SisternodeOutlined />
						</a-menu-item>
						<a-menu-item key="2" @click="addConditionUp(i)" class="katable-filter-action-rvs">
							<SisternodeOutlined />
						</a-menu-item>
						<a-menu-divider />
						<a-menu-item key="3" @click="addConditionDownGroup(i)">
							<SisternodeOutlined class="katable-filter-action-grp" />
						</a-menu-item>
						<a-menu-item key="4" @click="addConditionUpGroup(i)" class="katable-filter-action-rvs">
							<SisternodeOutlined class="katable-filter-action-grp" />
						</a-menu-item>
						<a-menu-divider />
						<a-menu-item :disabled="i === 0" key="5" @click="deleteCondition(i)">
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
import { PropType, computed, onBeforeMount, ref } from 'vue';
import KaTableInput from '../katable_input/KaTableInput.vue';
import { KaTableFilterCol, KaTableFilterCondition } from '.';
import { SettingOutlined, SisternodeOutlined, CloseOutlined } from '@ant-design/icons-vue';
import lodash from 'lodash';

const conditions = defineModel<KaTableFilterCondition[]>({});
// const conditions = ref<KaTableFilterCondition[]>([]);

const props = defineProps({
	columns: {
		type: Array as PropType<KaTableFilterCol[]>,
	},
});

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
	{ label: '列表', value: 'in' },
	{ label: '空', value: 'nu' },
	{ label: '不为空', value: 'nnu' },
];

const onChangeCol = (condition: KaTableFilterCondition, _value: any, col: KaTableFilterCol) => {
	console.log('onChangeCol');
	condition.opt = undefined;
	condition.type = col.type;
	condition.colLength = col.colLength;
	condition.val = '';
	condition.disabled = false;
	condition.multiple = false;
	condition.dateFormat = col.dateFormat;
	condition.options = getOptions(col?.type || 'string');
};

const onValChange = (condition: KaTableFilterCondition, value: any) => {
	console.log('onValChange');
	if (lodash.isArray(value)) {
		const col = props.columns!.find(col => col.col === condition.col);
		if (col!.type === 'number') {
			const lst = value[value.length - 1];
			if (condition.val[condition.val.length - 1] === lst) {
				condition.val[condition.val.length - 1] = parseFloat(lst);
			}
		}
	}
};

const onChangeOpt = (condition: KaTableFilterCondition, opt: KaTableFilterCondition['opt']) => {
	console.log('onChangeOpt');
	if (!opt) return;
	const col = props.columns!.find(col => col.col === condition.col);

	condition.dateFormat = col?.dateFormat;

	const valSingle = lodash.isArray(condition.val) ? condition.val[0] : condition.val;
	const valMult = lodash.isArray(condition.val)
		? condition.val
		: condition.val == null || condition.val == ''
		? []
		: [condition.val];

	if (['beg', 'end', 'like'].includes(opt!)) {
		condition.type = 'string';
		condition.disabled = false;
		condition.multiple = false;
		condition.valOptions = undefined;
		condition.val = valSingle;
		return;
	}
	if (opt === 'in') {
		if (col!.valOptions) {
			condition.valOptions = col!.valOptions;
		}
		condition.type = 'select';
		condition.disabled = false;
		condition.multiple = true;
		condition.val = valMult;
		return;
	}
	if (['nu', 'nnu'].includes(opt!)) {
		condition.type = 'string';
		condition.disabled = true;
		condition.multiple = false;
		condition.valOptions = undefined;
		condition.val = null;
		return;
	}

	if (['eq', 'neq', 'gt', 'gte', 'lt', 'lte'].includes(opt!)) {
		if (col!.valOptions) {
			condition.valOptions = col!.valOptions;
			condition.type = 'select';
			condition.disabled = false;
			condition.multiple = false;
			condition.val = valSingle;
			return;
		}
	}

	condition.type = col!.type;
	condition.disabled = false;
	condition.multiple = false;
	condition.valOptions = undefined;
	condition.val = valSingle;
};

const getOptions = (type: string) => {
	console.log('getOptions');
	if (type === 'number') {
		return optOptions.filter(opt => ['eq', 'neq', 'gt', 'gte', 'lt', 'lte', 'in', 'nu', 'nnu'].includes(opt.value));
	}
	if (type === 'date') {
		return optOptions.filter(opt => ['eq', 'neq', 'gt', 'gte', 'lt', 'lte', 'nu', 'nnu'].includes(opt.value));
	}
	return optOptions;
};


const createEmptyCondition = () => {
	return {
		col: '',
		val: '',
		bool: 'and',
	} as KaTableFilterCondition;
};
const createEmptyConditionGroup = () => {
	return {
		col: '',
		val: '',
		bool: 'and',
		children: [createEmptyCondition()],
	} as KaTableFilterCondition;
};

const addConditionUp = (index: number) => {
	conditions.value!.splice(index, 0, createEmptyCondition());
};
const addConditionDown = (index: number) => {
	conditions.value!.splice(index + 1, 0, createEmptyCondition());
};
const addConditionUpGroup = (index: number) => {
	conditions.value!.splice(index, 0, createEmptyConditionGroup());
};
const addConditionDownGroup = (index: number) => {
	conditions.value!.splice(index + 1, 0, createEmptyConditionGroup());
};
const deleteCondition = (index: number) => {
	conditions.value!.splice(index, 1);
};

const setConditions = (_conditions: KaTableFilterCondition[]) => {
	console.log('setConditions');
	if (_conditions!.length === 0) {
		_conditions!.push(createEmptyCondition());
		return;
	}
	_conditions![0].bool = 'and';

	for (const condition of _conditions!) {
		onChangeOpt(condition, condition.opt);
		condition.options = getOptions(condition?.type || 'string');
	}

	// console.log(_conditions)
	conditions.value = _conditions;
};

defineExpose({ setConditions });

onBeforeMount(() => {
	//reset(conditions.value!);
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
