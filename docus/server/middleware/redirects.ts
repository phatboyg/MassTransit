import {H3Event} from "h3";

const mapping : {[key:string]: string}= {
    '/bill': '/bob'
}
export default defineEventHandler((evt: H3Event) => {
    const path = evt.node.req.url || ''
    const dest = mapping[path]

    if(dest) {
        sendRedirect(evt, dest, 302)
            .then(() => {})
    }
})
