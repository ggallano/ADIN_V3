pipeline {
    agent { label 'IPN_Software_Agent' }

    stages {
        stage('Checkout') {
            steps {
                echo 'Checkout'
                //checkout([$class: 'GitSCM', branches: [[name: '*/${params.TARGET_BRANCH}']], browser: bitbucketServer('https://bitbucket.analog.com/scm/adin1100gu/adin1100gui.git'), extensions: [], userRemoteConfigs: [[credentialsId: 'ggallan2', url: 'https://bitbucket.analog.com/scm/adin1100gu/adin1100gui.git']]])
            }
        }
        
		stage('Source_Code_Clean') {
		    steps {
				//dir('examples\\adin1100\\Eval-adin1100-ebz\\Eval-adin1100-ebz') {
				//	bat '''
				//		"c:\\Program Files (x86)\\IAR Systems\\Embedded Workbench 8.32.4\\common\\bin\\IarBuild.exe" Eval-adin1100-ebz.ewp -build Debug
				//		if %errorlevel%==-1073741819 (EXIT 0) else (EXIT %errorlevel%)
				//	'''
				//}

				//bat "msbuild.exe ${WORKSPACE}/${SOLUTION_PATH}/${SOLUTION_NAME}.sln" /nologo /nr:false /p:platform=\"x64\" /p:configuration=\"release\" /t:clean"
				//bat "echo Source_Code_Clean"
				echo 'Source_Code_Clean'
			}
		}
		
		stage('Source_Code_Build') {
            steps {
				//bat "msbuild.exe ${WORKSPACE}/${SOLUTION_PATH}/${SOLUTION_NAME}.sln /nologo /nr:false  /p:platform=\"x64\" /p:configuration=\"release\" /p:PackageCertificateKeyFile=<path-to-certificate-file>.pfx /t:clean;restore;rebuild"
				
				//bat "echo Source_Code_Build"
				echo 'Source_Code_Build'
            }
        }
		
		stage('Installer_Creation') {
			steps {
				//bat """
				//	/* Insert Command Line Here */
				//"""
				
				//bat "echo Installer_Creation"
				echo 'Installer_Creation'
			}
		}
		
		//stage('Publish') {
		//	steps {
		//		bat "echo Publish"
		//	}
		//}
    }
}
