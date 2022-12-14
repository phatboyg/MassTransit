<script setup>
const navigationTree = await queryContent('documentation', 'configuration').find();
</script>

<template>
  <ul class="navigation">
    <li v-for="(item, index) in navigationTree" :key="index">
      <NuxtLink :to="item._path">
        {{ item.title }}
      </NuxtLink>
      <FolderNavigation v-if="item.children" :navigation-tree="item.children" class="sub-navigation" />
    </li>
  </ul>
</template>

<style scoped>
.navigation li {
  list-style-type: '- ';
}

.navigation ul {
  margin-left: .8rem;
}
</style>
