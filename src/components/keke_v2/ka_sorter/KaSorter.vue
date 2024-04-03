<template>
	<ka-drag-list ref="$list">
		<template #renderItem="{ item }">
			<span :style="{ color: props.primaryColor }" style="width: 1rem">
				<sort-ascending-outlined v-show="item.order === 'ascend'" />
				<sort-descending-outlined v-show="item.order === 'descend'" />
			</span>

			{{ props.colObj[item.key] }}
		</template>
		<template #renderAction="{ item }">
			<a-radio-group v-model:value="item.order" @change="item.disabled = item.order === null" button-style="solid">
				<a-radio-button value="ascend"><sort-ascending-outlined /></a-radio-button>
				<a-radio-button value="descend"><sort-descending-outlined /></a-radio-button>
				<a-radio-button :value="null"><minus-outlined /></a-radio-button>
			</a-radio-group>
		</template>
	</ka-drag-list>
</template>

<script setup lang="ts">
import { PropType, ref } from 'vue';
import KaDragList from '../ka_drag_list/KaDragList.vue';
import { KaSorterCondition } from '.';
import { SortAscendingOutlined, SortDescendingOutlined, MinusOutlined } from '@ant-design/icons-vue';

const props = defineProps({
	primaryColor: {
		type: String,
	},
	colObj: {
		type: Object as PropType<{ [key: string]: string }>,
		required: true,
	},
});

const $list = ref<InstanceType<typeof KaDragList>>();

defineExpose({
	setSorter: (list: KaSorterCondition[]) => {
		$list.value?.setSorter(list.map(item => ({ ...item, disabled: item.order === null })));
	},
	getSorter: () => {
		let i = 0;
		return $list.value
			?.getSorter()
			.map(
				l => ({ key: l.key, index: l.order === null ? null : ++i, order: l.order } as KaSorterCondition)
			) as KaSorterCondition[];
	},
});
</script>

<style scoped></style>
