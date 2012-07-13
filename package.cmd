if not exist Download mkdir Download
if not exist Download\packages mkdir Download\packages
if not exist Download\packages\Faker mkdir Download\packages\Faker
if not exist Download\packages\Faker\lib mkdir Download\packages\Faker\lib
if not exist Download\packages\Faker\lib\net4 mkdir Download\packages\Faker\lib\net4
if not exist Download\packages\Faker\lib\net35 mkdir Download\packages\Faker\lib\net35

copy LICENSE.txt Download\packages\Faker

copy Faker\bin\Release\Faker.dll Download\packages\Faker\lib\net4
copy Faker.Net35\bin\Release\Faker.Net35.dll Download\packages\Faker\lib\net35

nuget.exe pack Faker.nuspec -BasePath Download\packages\Faker -OutputDirectory Download