using Amazon.CDK;
using Amazon.CDK.AWS.CloudWatch;
using Amazon.CDK.AWS.CloudWatch.Actions;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.SSM;
using Amazon.JSII.JsonModel.FileSystem;
using Constructs;
using System;
using System.Collections.Generic;

namespace QueTalMiAfpNotificacionesCdk
{
    public class QueTalMiAfpNotificacionesCdkStack : Stack
    {
        internal QueTalMiAfpNotificacionesCdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            string appName = System.Environment.GetEnvironmentVariable("APP_NAME") ?? throw new ArgumentNullException("APP_NAME");
            string region = System.Environment.GetEnvironmentVariable("REGION_AWS") ?? throw new ArgumentNullException("REGION_AWS");

            string notificacionLambdaDirectory = System.Environment.GetEnvironmentVariable("NOTIFICACION_LAMBDA_DIRECTORY") ?? throw new ArgumentNullException("NOTIFICACION_LAMBDA_DIRECTORY");
            string notificacionLambdaHandler = System.Environment.GetEnvironmentVariable("NOTIFICACION_LAMBDA_HANDLER") ?? throw new ArgumentNullException("NOTIFICACION_LAMBDA_HANDLER");
            string notificacionLambdaMemorySize = System.Environment.GetEnvironmentVariable("NOTIFICACION_LAMBDA_MEMORY_SIZE") ?? throw new ArgumentNullException("NOTIFICACION_LAMBDA_MEMORY_SIZE");
            string notificacionLambdaTimeout = System.Environment.GetEnvironmentVariable("NOTIFICACION_LAMBDA_TIMEOUT") ?? throw new ArgumentNullException("NOTIFICACION_LAMBDA_TIMEOUT");

            string arnParameterApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_API_URL") ?? throw new ArgumentNullException("ARN_PARAMETER_API_URL");
            string arnParameterApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_API_KEY_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_API_KEY_ID");
            string arnParameterDireccionDeDefecto = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_DIRECCION_DE_DEFECTO") ?? throw new ArgumentNullException("ARN_PARAMETER_DIRECCION_DE_DEFECTO");
            string arnParameterKairosExecutorPrefixRole = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_KAIROS_EXECUTOR_PREFIX_ROLE") ?? throw new ArgumentNullException("ARN_PARAMETER_KAIROS_EXECUTOR_PREFIX_ROLE");
            string arnParameterKairosExecutorRoleArn = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_KAIROS_EXECUTOR_ROLE_ARN") ?? throw new ArgumentNullException("ARN_PARAMETER_KAIROS_EXECUTOR_ROLE_ARN");
            string arnParameterHermesApiUrl = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_URL") ?? throw new ArgumentNullException("ARN_PARAMETER_HERMES_API_URL");
            string arnParameterHermesApiKeyId = System.Environment.GetEnvironmentVariable("ARN_PARAMETER_HERMES_API_KEY_ID") ?? throw new ArgumentNullException("ARN_PARAMETER_HERMES_API_KEY_ID");

            string notificationEmails = System.Environment.GetEnvironmentVariable("NOTIFICATION_EMAILS") ?? throw new ArgumentNullException("NOTIFICATION_EMAILS");

            #region SNS Topic
            // Se crea SNS topic para notificaciones...
            Topic topic = new(this, $"{appName}NotificacionesSNSTopic", new TopicProps {
                TopicName = $"{appName}NotificacionesSNSTopic",
            });

            foreach (string email in notificationEmails.Split(",")) {
                topic.AddSubscription(new EmailSubscription(email));
            }
            #endregion

            #region DLQ y Alarms
            // Creación de cola...
            Queue dlq = new(this, $"{appName}NotificacionesDeadLetterQueue", new QueueProps {
                QueueName = $"{appName}NotificacionesDeadLetterQueue",
                RetentionPeriod = Duration.Days(14),
                EnforceSSL = true,
            });

            // Se crea alarma para enviar notificación cuando llegue un elemento al DLQ...
            Alarm alarm = new(this, $"{appName}NotificacionesDeadLetterQueueAlarm", new AlarmProps {
                AlarmName = $"{appName}NotificacionesDeadLetterQueueAlarm",
                AlarmDescription = $"Alarma para notificar cuando llega algun elemento a la DLQ de Notificaciones {appName}",
                Metric = dlq.MetricApproximateNumberOfMessagesVisible(new MetricOptions {
                    Period = Duration.Minutes(5),
                    Statistic = Stats.MAXIMUM,
                }),
                Threshold = 1,
                EvaluationPeriods = 1,
                DatapointsToAlarm = 1,
                ComparisonOperator = ComparisonOperator.GREATER_THAN_OR_EQUAL_TO_THRESHOLD,
                TreatMissingData = TreatMissingData.NOT_BREACHING,
            });
            alarm.AddAlarmAction(new SnsAction(topic));
            #endregion

            #region Log Group y Role
            // Creación de log group lambda...
            LogGroup lambdaLogGroup = new(this, $"{appName}NotificacionesLogGroup", new LogGroupProps {
                LogGroupName = $"/aws/lambda/{appName}Notificaciones/logs",
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // Se obtiene ID de API Keys...
            IStringParameter strParHermesApiKeyId = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterHermesApiKeyId", arnParameterHermesApiKeyId);
            IStringParameter strPaApiKeyId = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterApiKeyId2", arnParameterApiKeyId);

            // Creación de role para la función lambda...
            Role roleLambda = new(this, $"{appName}NotificacionesLambdaRole", new RoleProps {
                RoleName = $"{appName}NotificacionesLambdaRole",
                Description = $"Role para Lambda de Notificaciones de {appName}",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = [
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaVPCAccessExecutionRole"),
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole"),
                ],
                InlinePolicies = new Dictionary<string, PolicyDocument> {
                    {
                        $"{appName}NotificacionesLambdaPolicy",
                        new PolicyDocument(new PolicyDocumentProps {
                            Statements = [
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToParameterStore",
                                    Actions = [
                                        "ssm:GetParameter"
                                    ],
                                    Resources = [
                                        arnParameterApiUrl,
                                        arnParameterApiKeyId,
                                        arnParameterHermesApiUrl,
                                        arnParameterHermesApiKeyId,
                                        arnParameterDireccionDeDefecto,
                                    ],
                                }),
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToApiKey",
                                    Actions = [
                                        "apigateway:GET"
                                    ],
                                    Resources = [
                                        $"arn:aws:apigateway:{this.Region}::/apikeys/{strParHermesApiKeyId.StringValue}",
                                        $"arn:aws:apigateway:{this.Region}::/apikeys/{strPaApiKeyId.StringValue}",
                                    ],
                                }),
                            ]
                        })
                    }
                }
            });
            #endregion

            #region Lambda
            // Creación de la función lambda...
            Function function = new(this, $"{appName}NotificacionesLambdaFunction", new FunctionProps {
                FunctionName = $"{appName}NotificacionesLambdaFunction",
                Description = $"Lambda encargada de enviar las notificaciones de la aplicacion {appName}",
                Runtime = Runtime.DOTNET_8,
                Handler = notificacionLambdaHandler,
                Code = Code.FromAsset($"{notificacionLambdaDirectory}/publish/publish.zip"),
                Timeout = Duration.Seconds(double.Parse(notificacionLambdaTimeout)),
                MemorySize = double.Parse(notificacionLambdaMemorySize),
                Architecture = Architecture.X86_64,
                LogGroup = lambdaLogGroup,
                Environment = new Dictionary<string, string> {
                    { "APP_NAME", appName },
                    { "ARN_PARAMETER_API_URL", arnParameterApiUrl },
                    { "ARN_PARAMETER_API_KEY_ID", arnParameterApiKeyId },
                    { "ARN_PARAMETER_HERMES_API_URL", arnParameterHermesApiUrl },
                    { "ARN_PARAMETER_HERMES_API_KEY_ID", arnParameterHermesApiKeyId },
                    { "ARN_PARAMETER_DIRECCION_DE_DEFECTO", arnParameterDireccionDeDefecto },
                },
                Role = roleLambda,
                DeadLetterQueueEnabled = true,
                DeadLetterQueue = dlq
            });
            #endregion

            #region Role de Ejecución
            // Se obtienen parámetros de Kairos...
            IStringParameter strParKairosExecutorPrefixRole = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterKairosExecutorPrefixRole", arnParameterKairosExecutorPrefixRole);
            IStringParameter strParKairosExecutorRoleArn = StringParameter.FromStringParameterArn(this, $"{appName}StringParameterKairosExecutorRoleArn", arnParameterKairosExecutorRoleArn);

            // Creación del Role de Ejecución...
            Role ejecucionRole = new(this, $"{appName}EjecucionNotificacionesLambdaRole", new RoleProps {
                RoleName = $"{strParKairosExecutorPrefixRole.StringValue}{appName}EjecucionNotificacionesLambdaRole",
                Description = $"Role para ejecutar Lambda de Notificaciones de {appName}",
                AssumedBy = new ArnPrincipal(strParKairosExecutorRoleArn.StringValue),
                InlinePolicies = new Dictionary<string, PolicyDocument> {
                    {
                        $"{appName}EjecucionNotificacionesLambdaPolicy",
                        new PolicyDocument(new PolicyDocumentProps {
                            Statements = [
                                new PolicyStatement(new PolicyStatementProps{
                                    Sid = $"{appName}AccessToLambda",
                                    Actions = [
                                        "lambda:InvokeFunction"
                                    ],
                                    Resources = [
                                        function.FunctionArn,
                                    ],
                                }),
                            ]
                        })
                    }
                }
            });
            #endregion

            #region Parameter Store
            // Creación de los string parameters...
            _ = new StringParameter(this, $"{appName}StringParameterLambdaArn", new StringParameterProps {
                ParameterName = $"/{appName}/Notificaciones/LambdaArn",
                Description = $"ARN de la Lambda de notificaciones de la aplicacion {appName}",
                StringValue = function.FunctionArn,
                Tier = ParameterTier.STANDARD,
            });

            _ = new StringParameter(this, $"{appName}StringParameterEjecucionRoleArn", new StringParameterProps {
                ParameterName = $"/{appName}/Notificaciones/EjecucionRoleArn",
                Description = $"ARN del Role de ejecución de la Lambda de notificaciones de la aplicacion {appName}",
                StringValue = ejecucionRole.RoleArn,
                Tier = ParameterTier.STANDARD,
            });
            #endregion
        }
    }
}
