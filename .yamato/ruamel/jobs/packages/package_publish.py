from ruamel.yaml.scalarstring import DoubleQuotedScalarString as dss
from ..utils.namer import package_job_id_publish, packages_filepath, package_job_id_pack, package_job_id_test
from ..utils.constants import PATH_PACKAGES


def get_job_definition(package, agent, platforms):
    dependencies = [f'{packages_filepath()}#{package_job_id_pack(package["id"])}']
    for platform in platforms:
        dependencies.append(f'{packages_filepath()}#{package_job_id_test(package["id"],  platform["name"],"trunk")}')

    job = {
        'name': f'Publish { package["name"]}',
        'agent': dict(agent),
        'dependencies': dependencies,
        'commands': [
            f'npm install upm-ci-utils@stable -g --registry https://api.bintray.com/npm/unity/unity-npm',
            f'upm-ci package publish --package-path {package["packagename"]}'
        ],
        'artifacts':{
            'packages':{
                'paths':[
                    dss(PATH_PACKAGES)
                ]
            }
        }
    }
    return job


class Package_PublishJob():
    
    def __init__(self, package, agent, platforms):
        self.package_id = package["id"]
        self.job_id = package_job_id_publish(package["id"])
        self.yml = get_job_definition(package, agent, platforms)

    
    
    