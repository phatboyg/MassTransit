COPYRIGHT = "Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al. - All rights reserved."

require File.dirname(__FILE__) + "/build_support/BuildUtils.rb"
require File.dirname(__FILE__) + "/build_support/util.rb"

include FileTest
require 'albacore'
require File.dirname(__FILE__) + "/build_support/ilmergeconfig.rb"
require File.dirname(__FILE__) + "/build_support/ilmerge.rb"

BUILD_NUMBER_BASE = '2.0.0'
PRODUCT = 'MassTransit'
CLR_TOOLS_VERSION = 'v4.0.30319'

BUILD_CONFIG = ENV['BUILD_CONFIG'] || "Release"
BUILD_CONFIG_KEY = ENV['BUILD_CONFIG_KEY'] || 'NET40'
BUILD_PLATFORM = ''
TARGET_FRAMEWORK_VERSION = (BUILD_CONFIG_KEY == "NET40" ? "v4.0" : "v3.5")
MSB_USE = (BUILD_CONFIG_KEY == "NET40" ? :net4 : :net35)
OUTPUT_PATH = (BUILD_CONFIG_KEY == "NET40" ? 'net-4.0' : 'net-3.5')

props = { 
  :src => File.expand_path("src"),
  :lib => File.expand_path("lib"),
  :build_support => File.expand_path("build_support"),
  :stage => File.expand_path("build_output"),
  :output => File.join( File.expand_path("build_output"), OUTPUT_PATH ),
  :artifacts => File.expand_path("build_artifacts"),
  :projects => ["MassTransit", "MassTransit.RuntimeServices"]
}

puts "Building for .NET Framework #{TARGET_FRAMEWORK_VERSION} in #{BUILD_CONFIG}-mode."
 
desc "Displays a list of tasks"
task :help do

  taskHash = Hash[*(`rake.bat -T`.split(/\n/).collect { |l| l.match(/rake (\S+)\s+\#\s(.+)/).to_a }.collect { |l| [l[1], l[2]] }).flatten] 
 
  indent = "                          "
  
  puts "rake #{indent}#Runs the 'default' task"
  
  taskHash.each_pair do |key, value|
    if key.nil?  
      next
    end
    puts "rake #{key}#{indent.slice(0, indent.length - key.length)}##{value}"
  end
end


desc "Cleans, compiles, il-merges, unit tests, prepares examples, packages zip and runs MoMA"
task :all => [:clean, :compile, :compile_samples, :ilmerge, :copy_services, :tests]

desc "**Default**, compiles and runs tests"
task :default => [:clean, :compile, :compile_samples, :ilmerge, :copy_services]

desc "**DOOES NOT CLEAR OUTPUT FOLDER**, compiles and runs tests"
task :unclean => [:compile, :ilmerge, :tests]

desc "Update the common version information for the build. You can call this task without building."
assemblyinfo :global_version do |asm|
  asm_version = BUILD_NUMBER_BASE + ".0"
  commit_data = get_commit_hash_and_date
  commit = commit_data[0]
  commit_date = commit_data[1]
  build_number = "#{BUILD_NUMBER_BASE}.#{Date.today.strftime('%y%j')}"
  tc_build_number = ENV["BUILD_NUMBER"]
  puts "##teamcity[buildNumber '#{build_number}-#{tc_build_number}']" unless tc_build_number.nil?
  
  # Assembly file config
  asm.product_name = PRODUCT
  asm.description = "Git commit hash: #{commit} - #{commit_date} - MassTransit is a distributed application framework for .NET  http://masstransit-project.com"
  asm.version = asm_version
  asm.file_version = build_number
  asm.custom_attributes :AssemblyInformationalVersion => "#{asm_version}",
	:ComVisibleAttribute => false,
	:CLSCompliantAttribute => false
  asm.copyright = COPYRIGHT
  asm.output_file = 'src/SolutionVersion.cs'
  asm.namespaces "System", "System.Reflection", "System.Runtime.InteropServices", "System.Security"
end

desc "Prepares the working directory for a new build"
task :clean do
	FileUtils.rm_rf props[:artifacts]
	FileUtils.rm_rf props[:stage]
	# work around latency issue where folder still exists for a short while after it is removed
	waitfor { !exists?(props[:stage]) }
	waitfor { !exists?(props[:artifacts]) }
	
	Dir.mkdir props[:stage]
	Dir.mkdir props[:artifacts]
end

task :compile_samples => [:compile, :build_starbucks, :build_distributor] do

end

desc "Cleans, versions, compiles the application and generates build_output/."
task :compile => [:global_version, :build] do
	puts 'Copying unmerged dependencies to output folder'

	copyOutputFiles File.join(props[:src], "MassTransit/bin/#{BUILD_CONFIG}"), "log4net.{dll,pdb,xml}", props[:output]
	copyOutputFiles File.join(props[:src], "MassTransit/bin/#{BUILD_CONFIG}"), "Magnum.{dll,pdb,xml}", props[:output]

	copyOutputFiles File.join(props[:src], "Persistence/MassTransit.NHibernateIntegration/bin/#{BUILD_CONFIG}"), "MassTransit.NHibernateIntegration.{dll,pdb,xml}", File.join(props[:output], "Persistence/NHibernate")
	outc = File.join(props[:output], "Containers")

	copyOutputFiles File.join(props[:src], "Containers/MassTransit.StructureMapIntegration/bin/#{BUILD_CONFIG}"), "MassTransit*.{dll,pdb,xml}", outc
	copyOutputFiles File.join(props[:src], "Containers/MassTransit.UnityIntegration/bin/#{BUILD_CONFIG}"), "MassTransit*.{dll,pdb,xml}", outc
	copyOutputFiles File.join(props[:src], "Containers/MassTransit.WindsorIntegration/bin/#{BUILD_CONFIG}"), "MassTransit*.{dll,pdb,xml}", outc
	copyOutputFiles File.join(props[:src], "Containers/MassTransit.NinjectIntegration/bin/#{BUILD_CONFIG}"), "MassTransit*.{dll,pdb,xml}", outc
	copyOutputFiles File.join(props[:src], "Containers/MassTransit.AutofacIntegration/bin/#{BUILD_CONFIG}"), "MassTransit*.{dll,pdb,xml}", outc
	
	outt = File.join(props[:output], "Transports")

	copyOutputFiles File.join(props[:src], "Transports/MassTransit.Transports.MSMQ/bin/#{BUILD_CONFIG}"), "MassTransit*.{dll,pdb,xml}", File.join(outt, "MSMQ")
	copyOutputFiles File.join(props[:src], "Transports/MassTransit.Transports.Nms/bin/#{BUILD_CONFIG}"), "MassTransit*.{dll,pdb,xml}", File.join(outt, "ActiveMQ")
	copyOutputFiles File.join(props[:src], "Transports/MassTransit.Transports.Nms/bin/#{BUILD_CONFIG}"), "Apache*.{dll,pdb,xml}", File.join(outt, "ActiveMQ")
	copyOutputFiles File.join(props[:src], "Transports/MassTransit.Transports.RabbitMq/bin/#{BUILD_CONFIG}"), "MassTransit*.{dll,pdb,xml}", File.join(outt, "RabbitMQ")
	copyOutputFiles File.join(props[:src], "Transports/MassTransit.Transports.RabbitMq/bin/#{BUILD_CONFIG}"), "RabbitMQ*.{dll,pdb,xml}", File.join(outt, "RabbitMQ")
end

task :ilmerge => [:ilmerge_masstransit] do
end


ilmerge :ilmerge_masstransit do |ilm|
	out = File.join(props[:output], 'MassTransit.dll')
	ilm.output = out
	ilm.internalize = File.join(props[:build_support], 'internalize.txt')
	ilm.working_directory = File.join(props[:src], "MassTransit/bin/#{BUILD_CONFIG}")
	ilm.target = :library
        ilm.use MSB_USE
	ilm.log = File.join( props[:src], "MassTransit","bin","#{BUILD_CONFIG}", 'ilmerge.log' )
	ilm.allow_dupes = true
	ilm.references = [ 'MassTransit.dll', 'Stact.dll', 'Newtonsoft.Json.dll']
end

desc "Copying Services"
task :copy_services => [:compile] do
	puts "Copying services"
	targ = File.join(props[:stage], 'Services', 'RuntimeServices')
	src = File.join(props[:src], "MassTransit.RuntimeServices/bin/#{BUILD_CONFIG}")

	copyOutputFiles src, "MassTransit.*.{dll,exe,config,log4net.xml,sdf}", targ
	copyOutputFiles props[:output], 'MassTransit.dll', targ
     	copyOutputFiles src, "Castle*.dll", targ	
     	copyOutputFiles src, "log4net.dll", targ	
     	copyOutputFiles src, "Magnum.dll", targ	
     	copyOutputFiles src, "FluentNHibernate.dll", targ	
     	copyOutputFiles src, "NHibernate*.dll", targ	
     	copyOutputFiles src, "Iesi.Collections.dll", targ	
     	copyOutputFiles src, "StructureMap.dll", targ	
     	copyOutputFiles src, "Topshelf.dll", targ	
	copyOutputFiles File.join(props[:lib], 'SqlCe'), '*', targ
	copyOutputFiles File.join(props[:lib], 'SqlCe', 'x86'), '*', File.join(targ, 'x86')
	copyOutputFiles File.join(props[:lib], 'SqlCe', 'x86', 'Microsoft.VC90.CRT'), '*', File.join(targ, 'x86', 'Microsoft.VC90.CRT')
	copyOutputFiles File.join(props[:lib], 'SqlCe', 'amd64'), '*', File.join(targ, 'amd64')
	copyOutputFiles File.join(props[:lib], 'SqlCe', 'amd64', 'Microsoft.VC90.CRT'), '*', File.join(targ, 'amd64', 'Microsoft.VC90.CRT')

	targ = File.join(props[:stage], 'Services', 'SystemView')
	src = File.join(props[:src], "MassTransit.SystemView/bin/#{BUILD_CONFIG}")

	copyOutputFiles src, "MassTransit.*.{dll,exe,config}", targ
	copyOutputFiles props[:output], 'MassTransit.dll', targ
     	copyOutputFiles src, "log4net.dll", targ	
     	copyOutputFiles src, "Magnum.dll", targ	
     	copyOutputFiles src, "StructureMap.dll", targ	

	targ = File.join(props[:stage], 'Services', 'SystemView2')
	src = File.join(props[:src], "MassTransit.SystemView2/bin/#{BUILD_CONFIG}")

	copyOutputFiles src, "MassTransit.*.{dll,exe,config}", targ
	copyOutputFiles props[:output], 'MassTransit.dll', targ
     	copyOutputFiles src, "log4net.dll", targ	
     	copyOutputFiles src, "Magnum.dll", targ	
     	copyOutputFiles src, "StructureMap.dll", targ	
     	copyOutputFiles src, "WPFToolkit.dll", targ	

	targ = File.join(props[:stage], 'Samples', 'Starbucks')
	src = File.join(props[:src], "Samples", "Starbucks")

	copyOutputFiles props[:output], "MassTransit.dll", targ

	copyOutputFiles File.join(src, "Starbucks.Customer/bin/#{BUILD_CONFIG}"), "{log4net,Magnum,MassTransit.StructureMapIntegration,MassTransit.Transports.Msmq,StructureMap}.dll", targ
	copyOutputFiles File.join(src, "Starbucks.Barista/bin/#{BUILD_CONFIG}"), "{MassTransit.WindsorIntegration,Castle.Windsor,Castle.Core,Topshelf}.dll", targ
	copyOutputFiles File.join(src, "Starbucks.Cashier/bin/#{BUILD_CONFIG}"), "{MassTransit.NinjectIntegration,Ninject}.dll", targ
	copyOutputFiles File.join(src, "Starbucks.Cashier/bin/#{BUILD_CONFIG}"), "Starbucks.Cashier.exe", targ
	copyOutputFiles File.join(src, "Starbucks.Cashier/bin/#{BUILD_CONFIG}"), "cashier.log4net.xml", targ
	copyOutputFiles File.join(src, "Starbucks.Barista/bin/#{BUILD_CONFIG}"), "Starbucks.Barista.exe", targ
	copyOutputFiles File.join(src, "Starbucks.Barista/bin/#{BUILD_CONFIG}"), "barista.log4net.xml", targ
	copyOutputFiles File.join(src, "Starbucks.Customer/bin/#{BUILD_CONFIG}"), "Starbucks.Customer.exe", targ
	copyOutputFiles File.join(src, "Starbucks.Customer/bin/#{BUILD_CONFIG}"), "customer.log4net.xml", targ
	copyOutputFiles File.join(src, "Starbucks.Customer/bin/#{BUILD_CONFIG}"), "Starbucks.Messages.dll", targ
	
	targ = File.join(props[:stage], 'Samples', 'Distributor')
	src = File.join(props[:src], "Samples", "Distributor")

	copyOutputFiles props[:output], "MassTransit.dll", targ

	copyOutputFiles File.join(src, "Grid.Distributor.Activator/bin/#{BUILD_CONFIG}"), "{log4net,Magnum,MassTransit.StructureMapIntegration,MassTransit.Transports.Msmq,StructureMap,Topshelf}.dll", targ
	copyOutputFiles File.join(src, "Grid.Distributor.Activator/bin/#{BUILD_CONFIG}"), "Grid.Distributor.Activator.exe", targ
	copyOutputFiles File.join(src, "Grid.Distributor.Activator/bin/#{BUILD_CONFIG}"), "Grid.Distributor.Shared.dll", targ
	copyOutputFiles File.join(src, "Grid.Distributor.Activator/bin/#{BUILD_CONFIG}"), "*.config", targ
	copyOutputFiles File.join(src, "Grid.Distributor.Worker/bin/#{BUILD_CONFIG}"), "Grid.Distributor.Worker.exe", targ
	copyOutputFiles File.join(src, "Grid.Distributor.Worker/bin/#{BUILD_CONFIG}"), "*.config", targ
end


#desc "Prepare examples"
#task :prepare_examples => [:compile] do#
#	puts "Preparing samples"
#	targ = File.join(props[:output], 'Services', 'clock' )
#	copyOutputFiles File.join(props[:src], "Samples/StuffOnAShelf/bin/#{BUILD_CONFIG}"), "clock.*", targ
#	copyOutputFiles File.join(props[:src], "Samples/StuffOnAShelf/bin/#{BUILD_CONFIG}"), "StuffOnAShelf.{dll}", targ
#	copyOutputFiles props[:output], "Topshelf.{dll}", targ
#	copyOutputFiles props[:output], "log4net.{dll,pdb}", targ
#	copy('doc/Using Shelving.txt', props[:output])
#	copy('doc/log4net.config.example', props[:output])
#	commit_data = get_commit_hash_and_date
#	what_commit = File.new File.join(props[:output], "#{commit_data[0]} - #{commit_data[1]}.txt"), "w"
#	what_commit.puts "The file name denotes what commit these files were built off of. You can also find that information in the assembly info accessible through code."
#	what_commit.close
#end

desc "Only compiles the application."
msbuild :build do |msb|
	msb.properties :Configuration => BUILD_CONFIG, 
	    :BuildConfigKey => BUILD_CONFIG_KEY,
	    :TargetFrameworkVersion => TARGET_FRAMEWORK_VERSION,
	    :Platform => 'Any CPU'
	msb.properties[:TargetFrameworkVersion] = TARGET_FRAMEWORK_VERSION unless BUILD_CONFIG_KEY == 'NET35'
	msb.use :net4 #MSB_USE
	msb.targets :Clean, :Build
	msb.solution = 'src/MassTransit.sln'
end

msbuild :build_starbucks do |msb|
	msb.properties :Configuration => "Build", 
	    :BuildConfigKey => BUILD_CONFIG_KEY,
	    :TargetFrameworkVersion => TARGET_FRAMEWORK_VERSION,
	    :Platform => 'Any CPU'
	msb.properties[:TargetFrameworkVersion] = TARGET_FRAMEWORK_VERSION unless BUILD_CONFIG_KEY == 'NET35'
	msb.use :net4 #MSB_USE
	msb.targets :Clean, :Build
	msb.solution = 'src/Samples/Starbucks/Starbucks.sln'
end

msbuild :build_distributor do |msb|
	msb.properties :Configuration => "Build", 
	    :BuildConfigKey => BUILD_CONFIG_KEY,
	    :TargetFrameworkVersion => TARGET_FRAMEWORK_VERSION,
	    :Platform => 'Any CPU'
	msb.properties[:TargetFrameworkVersion] = TARGET_FRAMEWORK_VERSION unless BUILD_CONFIG_KEY == 'NET35'
	msb.use :net4 #MSB_USE
	msb.targets :Clean, :Build
	msb.solution = 'src/Samples/Distributor/Grid.Distributor.sln'
end

def copyOutputFiles(fromDir, filePattern, outDir)
	FileUtils.mkdir_p outDir unless exists?(outDir)
	Dir.glob(File.join(fromDir, filePattern)){|file|
		copy(file, outDir) if File.file?(file)
	}
end

task :tests => [:unit_tests]

desc "Runs unit tests (integration tests?, acceptance-tests?) etc."
task :unit_tests => [:compile] do
	Dir.mkdir props[:artifacts] unless exists?(props[:artifacts])

	runner = NUnitRunner.new(File.join('lib', 'nunit', 'net-2.0',  "nunit-console#{(BUILD_PLATFORM.empty? ? '' : "-#{BUILD_PLATFORM}")}.exe"),
		'tests',
		TARGET_FRAMEWORK_VERSION,
		['/nothread', '/nologo', '/labels', "\"/xml=#{File.join(props[:artifacts], 'nunit-test-results.xml')}\""])

	runner.run ['MassTransit.Tests'].map{ |assem| "#{assem}.dll" }
end

desc "Target used for the CI server. It both builds, tests and packages."
task :ci => [:default, :package, :moma]

desc "ZIPs up the build results and runs the MoMA analyzer."
zip :package do |zip|
	zip.directories_to_zip = [props[:stage]]
	zip.output_file = "MassTransit-#{BUILD_NUMBER_BASE}.zip"
	zip.output_path = [props[:artifacts]]
end

desc "Runs the MoMA mono analyzer on the project files. Start the executable manually without --nogui to update the profiles once in a while though, or you'll always get the same report from the analyzer."
task :moma => [:compile] do
	puts "Analyzing project fitness for mono:"
	dlls = project_outputs(props).join(' ')
	sh "lib/MoMA/MoMA.exe --nogui --out #{File.join(props[:artifacts], 'MoMA-report.html')} #{dlls}"
end

# TODO: create tasks for installing and running samples!

desc "Builds the nuget package"
task :nuget => [:compile, :ilmerge] do
	sh "lib/nuget.exe pack nugets/MassTransit.nuspec -o build_artifacts"
	sh "lib/nuget.exe pack nugets/MassTransit.StructureMap.nuspec -o build_artifacts"
	sh "lib/nuget.exe pack nugets/MassTransit.Autofac.nuspec -o build_artifacts"
	sh "lib/nuget.exe pack nugets/MassTransit.Ninject.nuspec -o build_artifacts"
	sh "lib/nuget.exe pack nugets/MassTransit.Unity.nuspec -o build_artifacts"
	sh "lib/nuget.exe pack nugets/MassTransit.CastleWindsor.nuspec -o build_artifacts"
	sh "lib/nuget.exe pack nugets/MassTransit.NHibernate.nuspec -o build_artifacts"
	sh "lib/nuget.exe pack nugets/MassTransit.RabbitMQ.nuspec -o build_artifacts"
end

def project_outputs(props)
	props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.dll" }.
		concat( props[:projects].map{ |p| "src/#{p}/bin/#{BUILD_CONFIG}/#{p}.exe" } ).
		find_all{ |path| exists?(path) }
end




