$c = get-content "./Build/index.html"

#rem Set-Content   [-Path] <string[]>   [-Value] <Object[]>

$c = $c.replace('<body>', '<body style="background: black; color: #aaaaaa; font-family: system-ui;">')

$c = $c.replace('<div class="webgl-logo"></div>',
  '<div class="no-webgl-logo" style="width: 400px; float: left;"><a style="color: inherit;" href="http://discord.gg/PHXRWVf" target="_blank">Juniper&apos;s Path</a>, made with <a style="color: inherit;" href="https://unity.com" target="_blank">Unity</a></div>'
)

$c = $c.replace('<div class="title">Juniper</div>', '<div class="title"></div>')

$c = $c.replace('<div class="fullscreen" on', '<div class="fullscreen" style="cursor: pointer; filter: invert(1) hue-rotate(246deg) brightness(0.5);" on')

set-content "./Build/index.html" $c

%firebase-tools-instant-win% firebase deploy