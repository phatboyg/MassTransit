export default defineNuxtConfig({
    extends: '@nuxt-themes/docus',
    colorMode: {
        preference: 'dark'
    },
    content: {
        documentDriven: true,
        highlight: {
            theme: {
                dark: 'material-darker',
                default: 'material-lighter'
            },
            preload: ['json', 'shell', 'markdown', 'yaml', 'bash', 'csharp']
        },
        navigation: {
            fields: ['icon', 'titleTemplate', 'aside']
        }
    }
})
