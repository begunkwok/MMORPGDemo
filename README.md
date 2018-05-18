# MMORPGDemo 简介
MMORPGDemo是个人用于业余时间学习的一个游戏项目，美术用的是网上资源，不可用于商业用途。
目前最新的版本在Dev分支中，简单介绍用到到框架和技术等。

- **框架**
-底层框架使用的是[GameFramework](https://github.com/EllanJiang/GameFramework.git)。
该框架对游戏开发过程中常用模块进行了封装，很大程度地规范开发过程、加快开发速度并保证产品质量，是个很不错的框架，研究过程中学习到
了很多东西。

- **Lua热更新**
-Lua使用的是腾讯的XLua，除了将XLua集成到项目中之外，整合了Lua的Common模块，主要包括cjson库，socket库，Lua实现的Vector3
Quaternion等结构体，常用工具类，常用集合类，字符串操作类，以及全局变量检测和类实现等公共模块。
因为是个人学习Demo，没有大量使用Lua做业务，目前用到Lua写的只有一个登陆界面LoginForm.lua和一个用于Hotfix测试的HotfixTest.lua。

- **UI**
-UI没有使用Unity原生的UGUI，为了快速开发使用的是[FairyGUI](http://www.fairygui.com/)，集成到项目中没有大修改，就遵循框架
GameFramework的资源管理规范做了UI资源包的管理，界面系统的管理，以及对Lua的支持。


- **GamePlay**
-GamePlay这边主要实现了大体的人物系统，技能系统，战斗系统，属性系统，关卡系统。 
人物AI暂时使用的是FSM，后续学习完腾讯的Behaviac后，会替换成行为树方式。 
目前技能系统的实现使用的是自己写的简易行为树，基本满足技能的编辑，但是因为没有写编辑器，手写xml还是麻烦，后续一样换成Behaviac。
关卡模块，写了一个简易的关卡编辑器，可以编辑障碍物，传送门，怪物生成，地图事件的触发等。

- **TODO**
-后续目标：
-1.引入Behaviac，完善AI和技能系统。
-2.完善关卡编辑器。研究寻路算法，优化AI寻路。
-3.深入学习图形学，研究图像渲染，为项目中加入一些炫酷效果。
-4.优化项目。
