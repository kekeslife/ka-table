<template>
	<div>
		<span>input: </span>
		<ka-input
			v-model="value"
			@change="
				val => {
					changeValue = val;
				}
			"
		></ka-input>
	</div>
	<div>
		<span>input-number: </span>
		<ka-input
			v-model="value"
			component-type="number"
			@change="
				val => {
					changeValue = val;
				}
			"
			style="width: 100%"
			:precision="2"
		></ka-input>
	</div>
	<div>
		<span>textarea: </span>
		<ka-input
			v-model="value"
			component-type="textarea"
			@change="
				val => {
					changeValue = val;
				}
			"
			style="width: 100%"
		></ka-input>
	</div>
	<div>
		<span>date: </span>
		<ka-input
			v-model="dateValue"
			component-type="date"
			@change="
				val => {
					changeValue = val;
				}
			"
			style="width: 100%"
			:show-time="{ format: 'HH:mm' }"
			:format="'YYYY/MM/DD HH:mm'"
			:show-now="true"
			:show-today="true"
		></ka-input>
	</div>
	<div>
		<span>select: </span>
		<ka-input
			v-model="selectValue"
			component-type="select"
			@change="
				val => {
					changeValue = val;
				}
			"
			@search="
				key => {
					console.log('search');
					options = [
						{ label: `${key}1`, value: `${key}1` },
						{ label: `${key}2`, value: `${key}2` },
					];
				}
			"
			:options="options"
			style="width: 100%"
		></ka-input>
	</div>
	<div>
		<span>select multiple: </span>
		<ka-input
			v-model="multipleValue"
			component-type="select"
			@change="
				val => {
					changeValue = val;
				}
			"
			@search="
				key => {
					console.log('search');
					options = [
						{ label: `${key}1`, value: `${key}1` },
						{ label: `${key}2`, value: `${key}2` },
					];
				}
			"
			:options="options"
			mode="multiple"
			style="width: 100%"
		></ka-input>
	</div>
	<div>
		<span>自定义 cascader: </span>
		<ka-input
			v-model="multipleValue"
			@change="
				val => {
					changeValue = val;
				}
			"
			@search="
				key => {
					console.log('search');
					cascaderOptions = [
						{ label: `${key}1`, value: `${key}1`, children: [{ label: `${key}1-1`, value: `${key}1-1` }] },
						{ label: `${key}2`, value: `${key}2`, children: [{ label: `${key}2-1`, value: `${key}2-1` }] },
					];
				}
			"
			:options="cascaderOptions"
			:custom-component="markRaw(Cascader)"
            :debounce-delay="0"
            :value-converter="(val) => val.join('||')"
			style="width: 100%"
		></ka-input>
	</div>

	<div>
		<b>change value: {{ changeValue }}</b>
	</div>
</template>

<script setup lang="ts">
import { markRaw, ref } from 'vue';
import KaInput from './KaInput.vue';
import { Cascader } from 'ant-design-vue';

const value = ref();
const dateValue = ref();
const selectValue = ref();
const multipleValue = ref([]);
const changeValue = ref('');
const options = ref([{ label: '1', value: '1' }]);
const cascaderOptions = ref([{ label: '1', value: '1', children: [{ label: '1-1', value: '1-1' }] }]);
</script>

<style scoped></style>
