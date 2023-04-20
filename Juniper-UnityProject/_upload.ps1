$MyDir = [System.IO.Path]::GetDirectoryName($myInvocation.MyCommand.Definition)
cd $MyDir

$id = get-content "./Assets/Resources/BuildIdentifier.txt"

$c = get-content "./Build/index.html"

#rem Set-Content   [-Path] <string[]>   [-Value] <Object[]>

$c = $c.replace('<body>', '<body style="background: black; color: #aaaaaa; font-family: system-ui;">')

$c = $c.replace('<div class="webgl-logo"></div>',
  '<div class="no-webgl-logo" style="margin-top: 0; width: 400px; float: left;"><a style="color: inherit;" href="http://discord.gg/PHXRWVf" target="_blank">Juniper&apos;s Path</a>, made with <a style="color: inherit;" href="https://unity.com" target="_blank">Unity</a> <span style="font-size: 60%;">| Build ' + $id + '</span></div>'
)

$c = $c.replace('<div class="title">Juniper</div>', '<div class="title"></div>')

$c = $c.replace('<div class="fullscreen" on', '<div class="fullscreen" style="cursor: pointer; filter: invert(1) hue-rotate(246deg) brightness(0.5);" on')

set-content "./Build/index.html" $c
write-output "rewritten index.html"

$cJs = get-content "./Build/Build/UnityLoader.js"
$cJs = $cJs.replace('UnityLoader.SystemInfo.mobile', 'false')
set-content "./Build/Build/UnityLoader.js" $cJs

write-output "rewritten UnityLoader.js"

write-output "can be uploaded now"

# ./firebase-tools-instant-win deploy