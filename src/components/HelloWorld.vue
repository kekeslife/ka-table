<script setup lang="ts">
import { useAttrs } from 'vue';
import type { ButtonProps } from 'ant-design-vue';


var attrs = useAttrs() as ButtonProps;

function click() {
  console.log(attrs)
}

function sub() {
  console.log('sub');
}

defineExpose({ sub })

</script>

<template>
  <a-button v-bind="$attrs" @click="click">
    <template #icon>
      <DownloadOutlined />
    </template>
    <template v-for="(_v, k) in $slots" v-slot:[k] :key="k">
      <slot :name="k"></slot>
    </template>
  </a-button>
</template>

<style scoped></style>
