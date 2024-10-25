echo // auto generated version: > version.h
echo #define VERSION   >> version.h
git log -1 --pretty=%%h >> version.h