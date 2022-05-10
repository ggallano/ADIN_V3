pipeline {
    agent { label 'CAV_SSE_Agent' }
	//agent { label 'IPN_Software_Agent' }

	environment {
		VERSION_NUMBER_FILE = "ADIN1100-Eval\\AutoVersionIncrement.cs"
		
		NSIS_PATH = "C:\\Program Files (x86)\\NSIS\\makensis.exe"
		MSBUILD_PATH = "C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\msbuild.exe"
		NUGET_PATH = "C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\nuget.exe"
		
		PROJECT_FILES = "ADIN1100-Eval;DeviceCommunication;TargetInterface;Utilities"
		FRAMEWORK_SDK = "C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v10.0A\\bin\\NETFX 4.6.2 Tools"
		
		SOLUTION_NAME = "ADIN1100-Eval.sln"
		NSIS_SCRIPT_NAME = "ADIN1100_Eval.nsi"
		BASE_INSTALLER_NAME = "Analog Devices Ethernet PHY Installer"
		
		RECIPIENTS = 'glenn.gallano@analog.com, Wassim.Magnin@analog.com, hector.arroyo@analog.com, danail.baylov@analog.com'
		//RECIPIENTS = 'glenn.gallano@analog.com'
		//DEV_RECIPIENTS = 'glenn.gallano@analog.com'
	}

    stages {
        //stage('Checkout') {
        //    steps {
        //        echo 'Checkout'
        //        //checkout([$class: 'GitSCM', branches: [[name: '*/${params.TARGET_BRANCH}']], browser: bitbucketServer('https://bitbucket.analog.com/scm/adin1100gu/adin1100gui.git'), extensions: [], userRemoteConfigs: [[credentialsId: 'ggallan2', url: 'https://bitbucket.analog.com/scm/adin1100gu/adin1100gui.git']]])
        //    }
        //}
        
		//stage('Source_Code_Clean') {
		//    steps {
		//		//dir('examples\\adin1100\\Eval-adin1100-ebz\\Eval-adin1100-ebz') {
		//		//	bat '''
		//		//		"c:\\Program Files (x86)\\IAR Systems\\Embedded Workbench 8.32.4\\common\\bin\\IarBuild.exe" Eval-adin1100-ebz.ewp -build Debug
		//		//		if %errorlevel%==-1073741819 (EXIT 0) else (EXIT %errorlevel%)
		//		//	'''
		//		//}
		//
		//		//bat "msbuild.exe ${WORKSPACE}/${SOLUTION_PATH}/${SOLUTION_NAME}.sln" /nologo /nr:false /p:platform=\"x64\" /p:configuration=\"release\" /t:clean"
		//		//bat "echo Source_Code_Clean"
		//		echo 'Source_Code_Clean'
		//	}
		//}
		
		//stage('Cleanup Workspace') {
        //    steps {
        //        cleanWs()
        //        bat """
		//			echo "Cleaned Up Workspace for ${APP_NAME}"
        //        """
        //    }
        //}
		
		stage('Source_Code_Build') {
            steps {
				echo 'Source_Code_Build'
				bat """
					echo "[DEBUG] Start Nuget Packages Restore"
						"${env.NUGET_PATH}" restore
					echo "End Nuget Packages Restore"
					
					echo "Start Release Build"
						"${env.MSBUILD_PATH}" ${env.SOLUTION_NAME} /property:Configuration=Release /p:VisualStudioVersion=14.0 /p:TargetFrameworkSDKToolsDirectory="${env.FRAMEWORK_SDK}" -t:${env.PROJECT_FILES} /p:Platform="Any CPU"
					echo "[DEBUG] End Release Build"
				"""
            }
        }
		
		stage('Retrieve Version Number'){
			steps {
				echo "[DEBUG] Start Retrieve Version Number"
				script {
					def fileContent = readFile "${env.VERSION_NUMBER_FILE}"
					def list = fileContent.split("\n")
					
					env.majorNumber = 0
					env.minorNumber = 0
					env.buildNumber = 0
					env.revisionNumber = 0
					
					for (int i = 0; i < list.size(); i++) {
						def line = list[i].trim()
						
						if (line =~ /\[assembly\:/)
						{
							def m = line =~ /\d+/
							
							env.majorNumber = m[0]
							env.minorNumber = m[1]
							env.buildNumber = m[2]
							env.revisionNumber = m[3]
							
							echo "[DEBUG] Version Number Retrieved Success, Version Number: ${env.majorNumber}.${env.minorNumber}.${env.buildNumber}.${env.revisionNumber}"
						}
					}
				}
				echo "[DEBUG] End Retrieve Version Number"
			}
		}
		
		stage('Installer_Creation') {
			steps {
				echo "[DEBUG] Start Installer_Creation"
					bat """
						"${env.NSIS_PATH}" /DVERSION=${env.majorNumber}.${env.minorNumber}.${env.buildNumber}.${env.revisionNumber} Installer\\${env.NSIS_SCRIPT_NAME}
					"""
				echo "[DEBUG] End Installer_Creation"
			}
		}
		
		stage('Publish') {
			steps {
				echo "[DEBUG] Start Publish"
					script {
						env.versionNumber = "${env.majorNumber}.${env.minorNumber}.${env.buildNumber}.${env.revisionNumber}"
						env.releaseInstallerName = "${env.BASE_INSTALLER_NAME}_v${env.versionNumber}"
					}
					
					bat """
						copy "Installer\\${env.BASE_INSTALLER_NAME}.exe" "Installer\\Archive\\${env.releaseInstallerName}.exe" /Y
					"""
					
					archiveArtifacts artifacts: "Installer/Archive/${env.releaseInstallerName}.exe"
				echo "[DEBUG] End Publish"
			}
		}
	}

	post {
		always {
			echo "[DEBUG] Post Always"
			
		}
	
		success {
			//mail to: "${env.RECIPIENTS}",
			//	subject: "ADIN1100GUI Latest Version [SUCCESS]", 
			//	body: """Hi\nKindly download the file under Build Artifacts in this link: ${env.BUILD_URL}"""
				
			echo "[DEBUG] Post Success"
		}
		
		failure {
			//mail to: "${env.DEV_RECIPIENTS}",
			//	subject: "ADIN1100GUI Latest Version [FAILED]", 
			//	body: """The build was failed.\n${env.BUILD_URL}"""
				
			echo "[DEBUG] Post Failure"
		}
	}
}
