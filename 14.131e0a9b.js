(window.webpackJsonp=window.webpackJsonp||[]).push([[14,15],{118:function(e,t,a){"use strict";const n=(e,{target:t=document.body}={})=>{const a=document.createElement("textarea"),n=document.activeElement;a.value=e,a.setAttribute("readonly",""),a.style.contain="strict",a.style.position="absolute",a.style.left="-9999px",a.style.fontSize="12pt";const r=document.getSelection();let o=!1;r.rangeCount>0&&(o=r.getRangeAt(0)),t.append(a),a.select(),a.selectionStart=0,a.selectionEnd=e.length;let l=!1;try{l=document.execCommand("copy")}catch(c){}return a.remove(),o&&(r.removeAllRanges(),r.addRange(o)),n&&n.focus(),l};e.exports=n,e.exports.default=n},119:function(e,t){function a(e){let t,a=[];for(let n of e.split(",").map((e=>e.trim())))if(/^-?\d+$/.test(n))a.push(parseInt(n,10));else if(t=n.match(/^(-?\d+)(-|\.\.\.?|\u2025|\u2026|\u22EF)(-?\d+)$/)){let[e,n,r,o]=t;if(n&&o){n=parseInt(n),o=parseInt(o);const e=n<o?1:-1;"-"!==r&&".."!==r&&"\u2025"!==r||(o+=e);for(let t=n;t!==o;t+=e)a.push(t)}}return a}t.default=a,e.exports=a},77:function(e,t,a){"use strict";a.r(t);var n=a(0),r=a.n(n),o=a(85),l=a(81),c=a(23),i=a(89),s=a(3),u=a(7),d=a(80),m=a(79),p=a(95),h=a(99),b=a(100),y=a(98),f=a(83),g=a(88),v=a(101),k=a(62),j=a.n(k);var E=function e(t,a){return"link"===t.type?Object(m.isSamePath)(t.href,a):"category"===t.type&&t.items.some((function(t){return e(t,a)}))};function O(e){var t,a,o,l=e.item,c=e.onItemClick,i=e.collapsible,m=e.activePath,p=Object(u.a)(e,["item","onItemClick","collapsible","activePath"]),h=l.items,b=l.label,y=E(l,m),f=(a=y,o=Object(n.useRef)(a),Object(n.useEffect)((function(){o.current=a}),[a]),o.current),g=Object(n.useState)((function(){return!!i&&(!y&&l.collapsed)})),v=g[0],k=g[1],O=Object(n.useRef)(null),C=Object(n.useState)(void 0),x=C[0],w=C[1],S=function(e){var t;void 0===e&&(e=!0),w(e?(null===(t=O.current)||void 0===t?void 0:t.scrollHeight)+"px":void 0)};Object(n.useEffect)((function(){y&&!f&&v&&k(!1)}),[y,f,v]);var _=Object(n.useCallback)((function(e){e.preventDefault(),x||S(),setTimeout((function(){return k((function(e){return!e}))}),100)}),[x]);return 0===h.length?null:r.a.createElement("li",{className:Object(d.a)("menu__list-item",{"menu__list-item--collapsed":v}),key:b},r.a.createElement("a",Object(s.a)({className:Object(d.a)("menu__link",(t={"menu__link--sublist":i,"menu__link--active":i&&y},t[j.a.menuLinkText]=!i,t)),onClick:i?_:void 0,href:i?"#!":void 0},p),b),r.a.createElement("ul",{className:"menu__list",ref:O,style:{height:x},onTransitionEnd:function(){v||S(!1)}},h.map((function(e){return r.a.createElement(N,{tabIndex:v?"-1":"0",key:e.label,item:e,onItemClick:c,collapsible:i,activePath:m})}))))}function C(e){var t=e.item,a=e.onItemClick,n=e.activePath,o=(e.collapsible,Object(u.a)(e,["item","onItemClick","activePath","collapsible"])),l=t.href,c=t.label,i=E(t,n);return r.a.createElement("li",{className:"menu__list-item",key:c},r.a.createElement(f.a,Object(s.a)({className:Object(d.a)("menu__link",{"menu__link--active":i}),to:l},Object(g.a)(l)?{isNavLink:!0,exact:!0,onClick:a}:{target:"_blank",rel:"noreferrer noopener"},o),c))}function N(e){switch(e.item.type){case"category":return r.a.createElement(O,e);case"link":default:return r.a.createElement(C,e)}}var x=function(e){var t,a,o=e.path,l=e.sidebar,c=e.sidebarCollapsible,i=void 0===c||c,s=e.onCollapse,u=e.isHidden,f=Object(n.useState)(!1),g=f[0],k=f[1],E=Object(m.useThemeConfig)(),O=E.navbar.hideOnScroll,C=E.hideableSidebar,x=Object(p.a)().isAnnouncementBarClosed,w=Object(y.a)().scrollY;Object(h.a)(g);var S=Object(b.a)();return Object(n.useEffect)((function(){S===b.b.desktop&&k(!1)}),[S]),r.a.createElement("div",{className:Object(d.a)(j.a.sidebar,(t={},t[j.a.sidebarWithHideableNavbar]=O,t[j.a.sidebarHidden]=u,t))},O&&r.a.createElement(v.a,{tabIndex:-1,className:j.a.sidebarLogo}),r.a.createElement("div",{className:Object(d.a)("menu","menu--responsive","thin-scrollbar",j.a.menu,(a={"menu--show":g},a[j.a.menuWithAnnouncementBar]=!x&&0===w,a))},r.a.createElement("button",{"aria-label":g?"Close Menu":"Open Menu","aria-haspopup":"true",className:"button button--secondary button--sm menu__button",type:"button",onClick:function(){k(!g)}},g?r.a.createElement("span",{className:Object(d.a)(j.a.sidebarMenuIcon,j.a.sidebarMenuCloseIcon)},"\xd7"):r.a.createElement("svg",{"aria-label":"Menu",className:j.a.sidebarMenuIcon,xmlns:"http://www.w3.org/2000/svg",height:24,width:24,viewBox:"0 0 32 32",role:"img",focusable:"false"},r.a.createElement("title",null,"Menu"),r.a.createElement("path",{stroke:"currentColor",strokeLinecap:"round",strokeMiterlimit:"10",strokeWidth:"2",d:"M4 7h22M4 15h22M4 23h22"}))),r.a.createElement("ul",{className:"menu__list"},l.map((function(e){return r.a.createElement(N,{key:e.label,item:e,onItemClick:function(e){e.target.blur(),k(!1)},collapsible:i,activePath:o})})))),C&&r.a.createElement("button",{type:"button",title:"Collapse sidebar","aria-label":"Collapse sidebar",className:Object(d.a)("button button--secondary button--outline",j.a.collapseSidebarButton),onClick:s}))},w={plain:{backgroundColor:"#2a2734",color:"#9a86fd"},styles:[{types:["comment","prolog","doctype","cdata","punctuation"],style:{color:"#6c6783"}},{types:["namespace"],style:{opacity:.7}},{types:["tag","operator","number"],style:{color:"#e09142"}},{types:["property","function"],style:{color:"#9a86fd"}},{types:["tag-id","selector","atrule-id"],style:{color:"#eeebff"}},{types:["attr-name"],style:{color:"#c4b9fe"}},{types:["boolean","string","entity","url","attr-value","keyword","control","directive","unit","statement","regex","at-rule","placeholder","variable"],style:{color:"#ffcc99"}},{types:["deleted"],style:{textDecorationLine:"line-through"}},{types:["inserted"],style:{textDecorationLine:"underline"}},{types:["italic"],style:{fontStyle:"italic"}},{types:["important","bold"],style:{fontWeight:"bold"}},{types:["important"],style:{color:"#c4b9fe"}}]},S={Prism:a(21).a,theme:w};function _(e,t,a){return t in e?Object.defineProperty(e,t,{value:a,enumerable:!0,configurable:!0,writable:!0}):e[t]=a,e}function T(){return(T=Object.assign||function(e){for(var t=1;t<arguments.length;t++){var a=arguments[t];for(var n in a)Object.prototype.hasOwnProperty.call(a,n)&&(e[n]=a[n])}return e}).apply(this,arguments)}var P=/\r\n|\r|\n/,I=function(e){0===e.length?e.push({types:["plain"],content:"",empty:!0}):1===e.length&&""===e[0].content&&(e[0].empty=!0)},L=function(e,t){var a=e.length;return a>0&&e[a-1]===t?e:e.concat(t)},M=function(e,t){var a=e.plain,n=Object.create(null),r=e.styles.reduce((function(e,a){var n=a.languages,r=a.style;return n&&!n.includes(t)||a.types.forEach((function(t){var a=T({},e[t],r);e[t]=a})),e}),n);return r.root=a,r.plain=T({},a,{backgroundColor:null}),r};function B(e,t){var a={};for(var n in e)Object.prototype.hasOwnProperty.call(e,n)&&-1===t.indexOf(n)&&(a[n]=e[n]);return a}var D=function(e){function t(){for(var t=this,a=[],n=arguments.length;n--;)a[n]=arguments[n];e.apply(this,a),_(this,"getThemeDict",(function(e){if(void 0!==t.themeDict&&e.theme===t.prevTheme&&e.language===t.prevLanguage)return t.themeDict;t.prevTheme=e.theme,t.prevLanguage=e.language;var a=e.theme?M(e.theme,e.language):void 0;return t.themeDict=a})),_(this,"getLineProps",(function(e){var a=e.key,n=e.className,r=e.style,o=T({},B(e,["key","className","style","line"]),{className:"token-line",style:void 0,key:void 0}),l=t.getThemeDict(t.props);return void 0!==l&&(o.style=l.plain),void 0!==r&&(o.style=void 0!==o.style?T({},o.style,r):r),void 0!==a&&(o.key=a),n&&(o.className+=" "+n),o})),_(this,"getStyleForToken",(function(e){var a=e.types,n=e.empty,r=a.length,o=t.getThemeDict(t.props);if(void 0!==o){if(1===r&&"plain"===a[0])return n?{display:"inline-block"}:void 0;if(1===r&&!n)return o[a[0]];var l=n?{display:"inline-block"}:{},c=a.map((function(e){return o[e]}));return Object.assign.apply(Object,[l].concat(c))}})),_(this,"getTokenProps",(function(e){var a=e.key,n=e.className,r=e.style,o=e.token,l=T({},B(e,["key","className","style","token"]),{className:"token "+o.types.join(" "),children:o.content,style:t.getStyleForToken(o),key:void 0});return void 0!==r&&(l.style=void 0!==l.style?T({},l.style,r):r),void 0!==a&&(l.key=a),n&&(l.className+=" "+n),l}))}return e&&(t.__proto__=e),t.prototype=Object.create(e&&e.prototype),t.prototype.constructor=t,t.prototype.render=function(){var e=this.props,t=e.Prism,a=e.language,n=e.code,r=e.children,o=this.getThemeDict(this.props),l=t.languages[a];return r({tokens:function(e){for(var t=[[]],a=[e],n=[0],r=[e.length],o=0,l=0,c=[],i=[c];l>-1;){for(;(o=n[l]++)<r[l];){var s=void 0,u=t[l],d=a[l][o];if("string"==typeof d?(u=l>0?u:["plain"],s=d):(u=L(u,d.type),d.alias&&(u=L(u,d.alias)),s=d.content),"string"==typeof s){var m=s.split(P),p=m.length;c.push({types:u,content:m[0]});for(var h=1;h<p;h++)I(c),i.push(c=[]),c.push({types:u,content:m[h]})}else l++,t.push(u),a.push(s),n.push(0),r.push(s.length)}l--,t.pop(),a.pop(),n.pop(),r.pop()}return I(c),i}(void 0!==l?t.tokenize(n,l,a):[n]),className:"prism-code language-"+a,style:void 0!==o?o.root:{},getLineProps:this.getLineProps,getTokenProps:this.getTokenProps})},t}(n.Component),R=a(118),A=a.n(R),W=a(119),F=a.n(W),H={plain:{color:"#bfc7d5",backgroundColor:"#292d3e"},styles:[{types:["comment"],style:{color:"rgb(105, 112, 152)",fontStyle:"italic"}},{types:["string","inserted"],style:{color:"rgb(195, 232, 141)"}},{types:["number"],style:{color:"rgb(247, 140, 108)"}},{types:["builtin","char","constant","function"],style:{color:"rgb(130, 170, 255)"}},{types:["punctuation","selector"],style:{color:"rgb(199, 146, 234)"}},{types:["variable"],style:{color:"rgb(191, 199, 213)"}},{types:["class-name","attr-name"],style:{color:"rgb(255, 203, 107)"}},{types:["tag","deleted"],style:{color:"rgb(255, 85, 114)"}},{types:["operator"],style:{color:"rgb(137, 221, 255)"}},{types:["boolean"],style:{color:"rgb(255, 88, 116)"}},{types:["keyword"],style:{fontStyle:"italic"}},{types:["doctype"],style:{color:"rgb(199, 146, 234)",fontStyle:"italic"}},{types:["namespace"],style:{color:"rgb(178, 204, 214)"}},{types:["url"],style:{color:"rgb(221, 221, 221)"}}]},$=a(90),z=function(){var e=Object(m.useThemeConfig)().prism,t=Object($.a)().isDarkTheme,a=e.theme||H,n=e.darkTheme||a;return t?n:a},J=a(63),K=a.n(J),U=/{([\d,-]+)}/,V=function(e){void 0===e&&(e=["js","jsBlock","jsx","python","html"]);var t={js:{start:"\\/\\/",end:""},jsBlock:{start:"\\/\\*",end:"\\*\\/"},jsx:{start:"\\{\\s*\\/\\*",end:"\\*\\/\\s*\\}"},python:{start:"#",end:""},html:{start:"\x3c!--",end:"--\x3e"}},a=["highlight-next-line","highlight-start","highlight-end"].join("|"),n=e.map((function(e){return"(?:"+t[e].start+"\\s*("+a+")\\s*"+t[e].end+")"})).join("|");return new RegExp("^\\s*(?:"+n+")\\s*$")},Y=/(?:title=")(.*)(?:")/,q=function(e){var t=e.children,a=e.className,o=e.metastring,l=Object(m.useThemeConfig)().prism,c=Object(n.useState)(!1),i=c[0],u=c[1],p=Object(n.useState)(!1),h=p[0],b=p[1];Object(n.useEffect)((function(){b(!0)}),[]);var y=Object(n.useRef)(null),f=[],g="",v=z();if(Array.isArray(t)&&(t=t.join("")),o&&U.test(o)){var k=o.match(U)[1];f=F()(k).filter((function(e){return e>0}))}o&&Y.test(o)&&(g=o.match(Y)[1]);var j=a&&a.replace(/language-/,"");!j&&l.defaultLanguage&&(j=l.defaultLanguage);var E=t.replace(/\n$/,"");if(0===f.length&&void 0!==j){for(var O,C="",N=function(e){switch(e){case"js":case"javascript":case"ts":case"typescript":return V(["js","jsBlock"]);case"jsx":case"tsx":return V(["js","jsBlock","jsx"]);case"html":return V(["js","jsBlock","html"]);case"python":case"py":return V(["python"]);default:return V()}}(j),x=t.replace(/\n$/,"").split("\n"),w=0;w<x.length;){var _=w+1,T=x[w].match(N);if(null!==T){switch(T.slice(1).reduce((function(e,t){return e||t}),void 0)){case"highlight-next-line":C+=_+",";break;case"highlight-start":O=_;break;case"highlight-end":C+=O+"-"+(_-1)+","}x.splice(w,1)}else w+=1}f=F()(C),E=x.join("\n")}var P=function(){A()(E),u(!0),setTimeout((function(){return u(!1)}),2e3)};return r.a.createElement(D,Object(s.a)({},S,{key:String(h),theme:v,code:E,language:j}),(function(e){var t,a=e.className,n=e.style,o=e.tokens,l=e.getLineProps,c=e.getTokenProps;return r.a.createElement(r.a.Fragment,null,g&&r.a.createElement("div",{style:n,className:K.a.codeBlockTitle},g),r.a.createElement("div",{className:K.a.codeBlockContent},r.a.createElement("div",{tabIndex:0,className:Object(d.a)(a,K.a.codeBlock,"thin-scrollbar",(t={},t[K.a.codeBlockWithTitle]=g,t))},r.a.createElement("div",{className:K.a.codeBlockLines,style:n},o.map((function(e,t){1===e.length&&""===e[0].content&&(e[0].content="\n");var a=l({line:e,key:t});return f.includes(t+1)&&(a.className=a.className+" docusaurus-highlight-code-line"),r.a.createElement("div",Object(s.a)({key:t},a),e.map((function(e,t){return r.a.createElement("span",Object(s.a)({key:t},c({token:e,key:t})))})))})))),r.a.createElement("button",{ref:y,type:"button","aria-label":"Copy code to clipboard",className:Object(d.a)(K.a.copyButton),onClick:P},i?"Copied":"Copy")))}))},G=(a(64),a(65)),Q=a.n(G),X=function(e){return function(t){var a,n=t.id,o=Object(u.a)(t,["id"]),l=Object(m.useThemeConfig)().navbar.hideOnScroll;return n?r.a.createElement(e,o,r.a.createElement("a",{"aria-hidden":"true",tabIndex:-1,className:Object(d.a)("anchor",(a={},a[Q.a.enhancedAnchor]=!l,a)),id:n}),o.children,r.a.createElement("a",{"aria-hidden":"true",className:"hash-link",href:"#"+n,title:"Direct link to heading"},"#")):r.a.createElement(e,o)}},Z=a(66),ee=a.n(Z),te={code:function(e){var t=e.children;return"string"==typeof t?t.includes("\n")?r.a.createElement(q,e):r.a.createElement("code",e):t},a:function(e){return r.a.createElement(f.a,e)},pre:function(e){return r.a.createElement("div",Object(s.a)({className:ee.a.mdxCodeBlock},e))},h1:X("h1"),h2:X("h2"),h3:X("h3"),h4:X("h4"),h5:X("h5"),h6:X("h6")},ae=a(91),ne=a(86),re=a(67),oe=a.n(re);function le(e){var t,a,c,s,u=e.currentDocRoute,p=e.versionMetadata,h=e.children,b=Object(l.default)(),y=b.siteConfig,f=b.isClient,g=p.pluginId,v=p.permalinkToSidebar,k=p.docsSidebars,j=p.version,E=v[u.path],O=k[E],C=Object(n.useState)(!1),N=C[0],w=C[1],S=Object(n.useState)(!1),_=S[0],T=S[1],P=Object(n.useCallback)((function(){_&&T(!1),w(!N),!_&&window.matchMedia("(prefers-reduced-motion: reduce)").matches&&T(!0)}),[_]);return r.a.createElement(i.a,{key:f,searchMetadatas:{version:j,tag:Object(m.docVersionSearchTag)(g,j)}},r.a.createElement("div",{className:oe.a.docPage},O&&r.a.createElement("div",{className:Object(d.a)(oe.a.docSidebarContainer,(t={},t[oe.a.docSidebarContainerHidden]=N,t)),onTransitionEnd:function(e){e.currentTarget.classList.contains(oe.a.docSidebarContainer)&&N&&T(!0)},role:"complementary"},r.a.createElement(x,{key:E,sidebar:O,path:u.path,sidebarCollapsible:null===(a=null===(c=y.themeConfig)||void 0===c?void 0:c.sidebarCollapsible)||void 0===a||a,onCollapse:P,isHidden:_}),_&&r.a.createElement("div",{className:oe.a.collapsedDocSidebar,title:"Expand sidebar","aria-label":"Expand sidebar",tabIndex:0,role:"button",onKeyDown:P,onClick:P})),r.a.createElement("main",{className:oe.a.docMainContainer},r.a.createElement("div",{className:Object(d.a)("container padding-vert--lg",oe.a.docItemWrapper,(s={},s[oe.a.docItemWrapperEnhanced]=N,s))},r.a.createElement(o.a,{components:te},h)))))}t.default=function(e){var t=e.route.routes,a=e.versionMetadata,n=e.location,o=t.find((function(e){return Object(ne.matchPath)(n.pathname,e)}));return o?r.a.createElement(le,{currentDocRoute:o,versionMetadata:a},Object(c.a)(t)):r.a.createElement(ae.default,e)}},91:function(e,t,a){"use strict";a.r(t);var n=a(0),r=a.n(n),o=a(89);t.default=function(){return r.a.createElement(o.a,{title:"Page Not Found"},r.a.createElement("main",{className:"container margin-vert--xl"},r.a.createElement("div",{className:"row"},r.a.createElement("div",{className:"col col--6 col--offset-3"},r.a.createElement("h1",{className:"hero__title"},"Page Not Found"),r.a.createElement("p",null,"We could not find what you were looking for."),r.a.createElement("p",null,"Please contact the owner of the site that linked you to the original URL and let them know their link is broken.")))))}}}]);