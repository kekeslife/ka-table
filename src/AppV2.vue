<template>
	<a-button @click="cm = 'input'">KaInput</a-button>
	<a-button @click="cm = 'editor'">KaEditor</a-button>
	<a-button @click="cm = 'filter'">KaFilter</a-button>
	<a-button @click="cm = 'sorter'">KaDragList</a-button>
	<a-button @click="cm = 'table'">KaTable</a-button>
	<component :is="cmp[cm]"></component>
</template>

<script setup lang="ts">
import { markRaw, ref } from 'vue';
import KaInput from './components/keke_v2/ka_input/KaInputTest.vue';
import KaEditor from './components/keke_v2/ka_editor/KaEditorTest.vue';
import KaFilter from './components/keke_v2/ka_filter/KaFilterTest.vue';
import KaDragList from './components/keke_v2/ka_drag_list/KaDragListTest.vue';
import KaTable from './components/keke_v2/ka_table/KaTableTest.vue';
const cm = ref('table');
const cmp: { [key: string]: any } = {
	input: markRaw(KaInput),
	editor: markRaw(KaEditor),
	filter: markRaw(KaFilter),
	sorter: markRaw(KaDragList),
	table: markRaw(KaTable),
};

</script>

<style scoped></style>
