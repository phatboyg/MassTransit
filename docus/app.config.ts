export default defineAppConfig({
    github: {
        owner: 'MassTransit',
        repo: 'MassTransit',
        branch: 'develop'
    },
    docus: {
        title: 'MassTransit',
        description: 'An open-source distributed application framework for .NET',
        url: 'https://masstransit-project.com',
        socials: {
            twitter: 'mtproj',
            github: 'MassTransit/MassTransit'
        },
        aside: {
            level: 1,
            exclude: []
        },
        cover: {
            src: 'https://raw.githubusercontent.com/phatboyg/MassTransit/docus/docus/public/mt-logo-color.png',
            alt: 'MassTransit Logo',
        },
        header: {
            showLinkIcon: true,
            exclude: []
        },
        footer: {
            credits: {
                text: 'Copyright 2023 Chris Patterson',
                href: 'https://masstransit.io',
            },
            icons: [],
        },
    }
})
