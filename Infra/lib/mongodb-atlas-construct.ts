import { SecurityGroup } from "aws-cdk-lib/aws-ec2";
import { IKey } from "aws-cdk-lib/aws-kms";
import { Construct } from "constructs";
import { CfnDatabaseUser, CfnDatabaseUserPropsAwsiamType, RoleDefinition, ScopeDefinitionType } from 'awscdk-resources-mongodbatlas'

export interface MongoDBAtlasProps {
  roleArn: string;
  serviceSG: SecurityGroup;
  importedKey?: IKey;
  mongoDB: MongoDB

}

export interface MongoDB {
  projectId: string;
  clusterName: string;
  clusterURL: string;
  comment?: string;
  roles: RoleDefinition[],
  profile: string
}

export class MongoDBAtlasConstruct extends Construct {
  constructor(scope: Construct, id: string, props: MongoDBAtlasProps) {
    super(scope, id);

    const {
      roleArn, serviceSG, importedKey, mongoDB: {
        projectId,
        clusterName,
        clusterURL,
        roles,
        comment,
        profile
      } } = props

    // TODO Re-enable CfnProjectIpAccessList after MongoDB has resolved 
    // TODO the public Cluodformation extension issue
    // new CfnProjectIpAccessList(this, 'MongoDBAtlasNetworkAccess', {
    //   accessList: [{
    //     awsSecurityGroup: serviceSG.securityGroupId,
    //     comment,
    //   }],
    //   profile,
    //   projectId: projectId,
    // })

    new CfnDatabaseUser(this, 'MongoDBAtlasUser', {
      databaseName: '$external',
      projectId: projectId,
      roles,
      username: roleArn,
      awsiamType: CfnDatabaseUserPropsAwsiamType.ROLE,
      profile,
      scopes: [{
        name: clusterName,
        type: ScopeDefinitionType.CLUSTER
      }]
    })
  }
}