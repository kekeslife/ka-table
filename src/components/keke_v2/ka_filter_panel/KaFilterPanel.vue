<template>
	<a-collapse v-model:activeKey="openFilterPanel" class="ka-filter-panel">
		<a-collapse-panel key="1" header="操作">
			<a-form layout="inline" :model="formState" :label-col="{ style: { width: '150px' } }">
				<a-flex wrap="wrap" gap="small">
					<a-form-item v-for="item in props.columns" :key="item.key" :label="item.title" class="item-flex">
						<ka-input
							:component-type="formState[item.key].valComponent"
							v-bind="item.attrs"
							v-model="formState[item.key].val"
							:value-converter="item.valueConverter"
							:debounce-delay="item.debounceDelay"
							:style="{ width: formState[item.key].valComponent === 'dateRange' ? '220px' : item.width }"
							@change="
								(v: any) => {
									onValChange(item, v);
								}
							"
							@search="
								(v: any) => {
									onValSearch(item, v);
								}
							"
							:options="formState[item.key].valOptions"
						>
						</ka-input>
					</a-form-item>
					<a-space size="large">
						<a-button type="primary" @click="onFilter">筛选</a-button>
						<a-button @click="onReset">清空</a-button>
					</a-space>
				</a-flex>
			</a-form>
			<a-divider style="margin: 12px 0"></a-divider>
			<a-space size="large">
				<a-button v-if="props.toolbar.hasAdd" @click="onAdd">新增</a-button>
				<a-button v-if="props.toolbar.hasEdit" @click="onEdit" :disabled="props.isDisabled">编辑</a-button>
				<a-popconfirm  v-if="props.toolbar.hasRemove"
					:title="props.language?.removeConfirm"
					:ok-text="props.language?.confirm"
					:show-cancel="false"
					:disabled="props.isDisabled"
					@confirm="onRemove"
				>
					<a-button :disabled="props.isDisabled">删除</a-button>
				</a-popconfirm>
				<a-button v-if="props.toolbar.hasRefresh" @click="onRefresh">刷新</a-button>
			</a-space>
		</a-collapse-panel>
	</a-collapse>
</template>

<script setup lang="ts">
import { PropType, onBeforeMount, reactive, ref, watch } from 'vue';
import * as lodash from 'lodash-es';

import KaInput from '../ka_input/KaInput.vue';
import { KaTableLang, KaTablePropsToolbar } from '../ka_table';
import { KaFilterCol, KaFilterItem } from '../ka_filter/index.ts';

// const conditions = defineModel<KaFilterCondition[]>({});
// const filterItems = defineModel<KaFilterItem[]>();

// const filterItems = ref<KaFilterCondition[]>([]);

const formState = reactive<{ [key: string]: KaFilterItem }>({});

const props = defineProps({
	columns: {
		type: Array as PropType<KaFilterCol[]>,
		required: true,
	},
	toolbar:{
		type:Object as PropType<KaTablePropsToolbar>,
		required: true,
	},
	language: { type: Object as PropType<KaTableLang> },
	isDisabled: { type: Boolean, default: false },
});

const value = defineModel<boolean>();

const openFilterPanel = ref(['1']);

watch(value, n => {
	if (n) {
		openFilterPanel.value = ['1'];
	} else {
		openFilterPanel.value = [];
	}
});

const onValChange = async (col: KaFilterCol, value: any) => {
	let val = col.valueConverter ? await col.valueConverter(value) : value;
	formState[col.key].val = val;
};

const onValSearch = async (col: KaFilterCol, key: any) => {
	if (lodash.isFunction(col.options)) {
		formState[col.key].valOptions = await col.options(key);
	}
	// console.log(key);
};

const onFilter = () => {
	emit('commit', formState);
};
const onAdd = () => {
	emit('add');
};
const onEdit = () => {
	emit('edit');
};
const onRemove = () => {
	emit('remove');
};
const onRefresh = () => {
	emit('refresh');
};

const onReset = () => {
	for (let key in formState) {
		formState[key].val = null;
	}
	emit('reset');
};

const emit = defineEmits<{
	commit: [formState: { [key: string]: any }];
	add: [];
	edit: [];
	remove: [];
	refresh: [];
	reset: [];
}>();

const render = () => {
	for (let col of props.columns) {
		let componentType = col.componentType;
		if (componentType === 'textarea') componentType = 'input';
		else if (componentType === 'date') componentType = 'dateRange';

		formState[col.key] = {
			key: col.key,
			opt: 'eq',
			val: null,
			bool: 'and',
			valOptions: lodash.isArray(col.options) ? col.options : [],
			optOptions: [],
			valComponent: componentType,
		};
	}
};

onBeforeMount(() => {
	render();
});
</script>

<style scoped>
.bool {
	width: 60px;
	text-align: center;
}
.opt {
	width: 100px;
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
