MODNAME		:= NavHud
KSPDIR		:= ${HOME}/ksp/KSP_linux
MANAGED		:= ${KSPDIR}/KSP_Data/Managed
GAMEDATA	:= ${KSPDIR}/GameData
MODGAMEDATA	:= ${GAMEDATA}/${MODNAME}
PLUGINDIR	:= ${MODGAMEDATA}/Plugins

TARGETS		:= ${MODNAME}.dll

MOD_FILES := \
	AssemblyVersion.cs \
	AzimuthLines.cs \
	EdgeMarkers.cs \
	HeadingMarker.cs \
	MainBehaviour.cs \
	Markers.cs \
	NavHud.cs \
	TargetAlignmentMarker.cs \
	ToolbarWrapper.cs \
	Util.cs \
	Values.cs \
	ZenithLines.cs

RESGEN2		:= resgen2
GMCS		:= mcs
#GMCSFLAGS	:= -optimize -warnaserror
GMCSFLAGS	:= -optimize
GIT			:= git
TAR			:= tar
ZIP			:= zip

all: ${TARGETS}

#.PHONY: version
#version:
#	@./git-version.sh

info:
	@echo "${MODNAME} Build Information"
	@echo "    resgen2:    ${RESGEN2}"
	@echo "    gmcs:       ${GMCS}"
	@echo "    gmcs flags: ${GMCSFLAGS}"
	@echo "    git:        ${GIT}"
	@echo "    tar:        ${TAR}"
	@echo "    zip:        ${ZIP}"
	@echo "    KSP Data:   ${KSPDIR}"

${MODNAME}.dll: ${MOD_FILES}
	${GMCS} ${GMCSFLAGS} -t:library -lib:${MANAGED} \
		-r:Assembly-CSharp,Assembly-CSharp-firstpass \
		-r:UnityEngine,UnityEngine.UI \
		-lib:${GAMEDATA}/000_ClickThroughBlocker/Plugins,${GAMEDATA}/001_ToolbarControl/Plugins \
		-r:ClickThroughBlocker,ToolbarControl \
		-out:$@ $^

clean:
	rm -f ${TARGETS}

install: all
	mkdir -p ${PLUGINDIR}
	cp ${TARGETS} ${PLUGINDIR}

.PHONY: all clean install
