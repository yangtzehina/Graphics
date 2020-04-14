import sys, glob
import ruamel
from jobs.utils.namer import *
from jobs.projects.project_standalone import Project_StandaloneJob
from jobs.projects.project_standalone_build import Project_StandaloneBuildJob
from jobs.projects.project_not_standalone import Project_NotStandaloneJob
from jobs.projects.project_all import Project_AllJob
from jobs.editor.editor import Editor_PrimingJob
from jobs.packages.package_pack import Package_PackJob
from jobs.packages.package_publish import Package_PublishJob
from jobs.packages.package_test import Package_TestJob
from jobs.packages.package_test_dependencies import Package_TestDependenciesJob
from jobs.packages.publish_all import Package_PublishAllJob
from jobs.packages.test_all import Package_AllPackageCiJob
from jobs.abv.all_project_ci import ABV_AllProjectCiJob
from jobs.abv.all_project_ci_nightly import ABV_AllProjectCiNightlyJob
from jobs.abv.all_smoke_tests import ABV_AllSmokeTestsJob
from jobs.abv.smoke_test import ABV_SmokeTestJob
from jobs.abv.trunk_verification import ABV_TrunkVerificationJob


# TODO:
# - variables to store path names (change only once)
# - functions to generate job ids (and names)
# - see if less duplication possible among the jobs
# - squash reused cmd like npm install, artifacts links etc

def load_yml(filepath):
    with open(filepath) as f:
        return yaml.load(f)

def dump_yml(filepath, yml_dict):
    with open(filepath, 'w') as f:
        yaml.dump(yml_dict, f)

def create_project_specific_jobs(metafile_name):

    metafile = load_yml(metafile_name)
    project = metafile["project"]

    for platform in metafile['platforms']:
        for api in platform['apis']:

            yml = {}
            for editor in metafile['editors']:
                for test_platform in metafile['test_platforms']:

                    if test_platform["name"].lower() == 'standalone':
                        job = Project_StandaloneJob(project, editor, platform, api, test_platform)
                        yml[job.job_id] = job.yml
                        
                        if platform["standalone_split"]: # This bool can be removed once everything uses split (or make it automatically know if split is present or not)
                            job = Project_StandaloneBuildJob(project, editor, platform, api)
                            yml[job.job_id] = job.yml
                    elif platform["name"].lower() != "android": # skip for only android
                        job = Project_NotStandaloneJob(project, editor, platform, api, test_platform)
                        yml[job.job_id] = job.yml
                    
            # store yml per [project]-[platform]-[api]
            yml_file = project_filepath_specific(project["name"], platform["name"], api["name"])
            dump_yml(yml_file, yml)



def create_project_all_jobs(metafile_name):

    metafile = load_yml(metafile_name)

    yml = {}
    for editor in metafile['editors']:
        job = Project_AllJob(metafile["project"]["name"], editor, metafile["all"]["dependencies"])
        yml[job.job_id] = job.yml

    yml_file = project_filepath_all(metafile["project"]["name"])
    dump_yml(yml_file, yml)



def create_editor_job(metafile_name):

    metafile = load_yml(metafile_name)

    yml = {}
    for platform in metafile["platforms"]:
        for editor in metafile["editors"]:
            job = Editor_PrimingJob(platform, editor, metafile["agent"])
            yml[job.job_id] = job.yml

    dump_yml(editor_filepath(), yml)


def create_package_jobs(metafile_name):
    metafile = load_yml(metafile_name)
    yml = {}

    for package in metafile["packages"]:
        job = Package_PackJob(package)
        yml[job.job_id] = job.yml

        job = Package_PublishJob(package, metafile["platforms"])
        yml[job.job_id] = job.yml

    for editor in metafile["editors"]:
        for platform in metafile["platforms"]:
            for package in metafile["packages"]:
                job = Package_TestJob(package, platform, editor)
                yml[job.job_id] = job.yml

                job = Package_TestDependenciesJob(package, platform, editor)
                yml[job.job_id] = job.yml

    for editor in metafile["editors"]:
        job = Package_AllPackageCiJob(metafile["packages"], metafile["platforms"], editor)
        yml[job.job_id] = job.yml
    
    job = Package_PublishAllJob(metafile["packages"])
    yml[job.job_id] = job.yml

    dump_yml(packages_filepath(), yml)


def create_abv_jobs(metafile_name):
    metafile = load_yml(metafile_name)
    yml = {}

    for editor in metafile["editors"]:
        for test_platform in metafile['test_platforms']:
            job = ABV_SmokeTestJob(editor, test_platform)
            yml[job.job_id] = job.yml
        
        job = ABV_AllSmokeTestsJob(editor, metafile["test_platforms"])
        yml[job.job_id] = job.yml

        job = ABV_AllProjectCiJob(editor, metafile["projects"])
        yml[job.job_id] = job.yml

        job = ABV_AllProjectCiNightlyJob(editor, metafile["projects"], metafile["test_platforms"])
        yml[job.job_id] = job.yml

        job = ABV_TrunkVerificationJob(editor, metafile["projects"], metafile["test_platforms"])
        yml[job.job_id] = job.yml

    dump_yml(abv_filepath(), yml)


# TODO clean up the code, make filenames more readable/reuse, split things appropriately (eg editor, files, etc), fix scrip arguments, fix testplatforms (xr), ...
if __name__== "__main__":

    # configure yaml
    yaml = ruamel.yaml.YAML()
    yaml.width = 4096
    yaml.indent(offset=2, mapping=4, sequence=5)


    # create editor
    print(f'Running: editor')
    create_editor_job('config/_editor.metafile')

    # create package jobs
    print(f'Running: packages')
    create_package_jobs('config/_packages.metafile')

    # create abv
    print(f'Running: abv')
    create_abv_jobs('config/_abv.metafile')

    # create yml jobs for each specified project (universal, shadergraph, vfx_lwrp, ...)
    args = sys.argv
    projects = glob.glob('config/[!_]*.metafile') if 'all' in args else [f'config/{project}.metafile' for project in args[1:]]   
    for project_metafile in projects:
        print(f'Running: {project_metafile}')
        create_project_specific_jobs(project_metafile) # create jobs for testplatforms
        create_project_all_jobs(project_metafile) # create All_ job



