' <snippetcrudemailattachments>


Imports System.ServiceModel
Imports System.ServiceModel.Description
Imports System.Text

' These namespaces are found in the Microsoft.Xrm.Sdk.dll assembly
' found in the SDK\bin folder.
Imports Microsoft.Xrm.Sdk
Imports Microsoft.Xrm.Sdk.Query
Imports Microsoft.Xrm.Sdk.Discovery
Imports Microsoft.Xrm.Sdk.Messages
Imports Microsoft.Xrm.Sdk.Client


' This namespace is found in Microsoft.Crm.Sdk.Proxy.dll assembly
' found in the SDK\bin folder.
Imports Microsoft.Crm.Sdk.Messages

Namespace Microsoft.Crm.Sdk.Samples
    Public Class CRUDEmailAttachments
        #Region "Class Level Members"


        ''' <summary>
        ''' Stores the organization service proxy.
        ''' </summary>
        Private _serviceProxy As OrganizationServiceProxy

        ' Define the IDs needed for this sample.
        Private _emailId As Guid
        Private _emailAttachmentId(2) As Guid


        #End Region ' Class Level Members

        #Region "How To Sample Code"
        ''' <summary>
        ''' Create, Retrieve, Update and Delete an e-mail attachment.
        ''' <param name="serverConfig">Contains server connection information.</param>
        ''' <param name="promptforDelete">When True, the user will be prompted to delete all
        ''' created entities.</param>
        ''' </summary>
        Public Sub Run(ByVal serverConfig As ServerConnection.Configuration, ByVal promptForDelete As Boolean)
            Try
                ' Connect to the Organization service. 
                ' The using statement assures that the service proxy will be properly disposed.
                _serviceProxy = ServerConnection.GetOrganizationProxy(serverConfig)
                Using _serviceProxy
                    ' This statement is required to enable early-bound type support.
                    _serviceProxy.EnableProxyTypes()

                    CreateRequiredRecords()


                    ' Create three e-mail attachments
                    For i As Integer = 0 To 2
                        Dim _sampleAttachment As ActivityMimeAttachment = New ActivityMimeAttachment With { _
                            .ObjectId = New EntityReference(Email.EntityLogicalName, _emailId), .ObjectTypeCode = Email.EntityLogicalName, _
                            .Subject = String.Format("Sample Attachment {0}", i), _
                            .Body = Convert.ToBase64String(New ASCIIEncoding().GetBytes("Example Attachment")), _
                            .FileName = String.Format("ExampleAttachment{0}.txt", i)}

                        _emailAttachmentId(i) = _serviceProxy.Create(_sampleAttachment)
                    Next i

                    Console.WriteLine("Created three e-mail attachments for the e-mail activity.")

                    ' Retrieve an attachment including its id, subject, filename and body.
                    Dim _singleAttachment As ActivityMimeAttachment = CType(_serviceProxy.Retrieve(ActivityMimeAttachment.EntityLogicalName, _
                            _emailAttachmentId(0), New ColumnSet("activitymimeattachmentid", "subject", "filename", "body")),  _
                            ActivityMimeAttachment)

                    Console.WriteLine("Retrieved an email attachment, {0}.", _singleAttachment.FileName)

                    ' Update attachment
                    _singleAttachment.FileName = "ExampleAttachmentUpdated.txt"
                    _serviceProxy.Update(_singleAttachment)

                    Console.WriteLine("Updated the retrieved e-mail attachment to {0}.", _singleAttachment.FileName)

                    ' Retrieve all attachments associated with the email activity.
                    Dim _attachmentQuery As QueryExpression = New QueryExpression With {.EntityName = ActivityMimeAttachment.EntityLogicalName, _
                                                                                        .ColumnSet = New ColumnSet("activitymimeattachmentid")}

                    _attachmentQuery.Criteria = New FilterExpression()
                    _attachmentQuery.Criteria.AddCondition("objectid", ConditionOperator.Equal, {_emailId})
                    _attachmentQuery.Criteria.AddCondition("objecttypecode", ConditionOperator.Equal, {Email.EntityLogicalName})
                    _attachmentQuery.Criteria.FilterOperator = LogicalOperator.And
                    Dim results As EntityCollection = _serviceProxy.RetrieveMultiple(_attachmentQuery)

                    Console.WriteLine("Retrieved all the e-mail attachments.")


                    DeleteRequiredRecords(promptForDelete)
                End Using

            ' Catch any service fault exceptions that Microsoft Dynamics CRM throws.
            Catch fe As FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault)
                ' You can handle an exception here or pass it back to the calling method.
                Throw
            End Try
        End Sub

        ''' <summary>
        ''' This method creates any entity records that this sample requires.
        ''' Creates the email activity.
        ''' </summary>
        Public Sub CreateRequiredRecords()
            ' Create Email Activity
            Dim email As Email = New Email With {.Subject = "This is an example email", .ActivityId = Guid.NewGuid()}

            _emailId = _serviceProxy.Create(email)

            Console.WriteLine("An e-mail activity is created.")
        End Sub


        ''' <summary>
        ''' Deletes the custom entity record that was created for this sample.
        ''' <param name="prompt">Indicates whether to prompt the user 
        ''' to delete the entity created in this sample.</param>
        ''' </summary>
        Public Sub DeleteRequiredRecords(ByVal prompt As Boolean)
            Dim deleteRecords As Boolean = True

            If prompt Then
                Console.WriteLine(vbLf &amp; "Do you want these entity records deleted? (y/n)")
                Dim answer As String = Console.ReadLine()

                deleteRecords = (answer.StartsWith("y") OrElse answer.StartsWith("Y"))
            End If

            If deleteRecords Then
                For j As Integer = 0 To 2
                    _serviceProxy.Delete(ActivityMimeAttachment.EntityLogicalName, _emailAttachmentId(j))
                Next j
                _serviceProxy.Delete(Email.EntityLogicalName, _emailId)
                Console.WriteLine("Entity records have been deleted.")
            End If
        End Sub

        #End Region ' How To Sample Code

        #Region "Main"
        ''' <summary>
        ''' Main. Runs the sample and provides error output.
        ''' <param name="args">Array of arguments to Main method.</param>
        ''' </summary>
        Public Shared Sub Main(ByVal args() As String)
            Try
                ' Obtain the target organization's Web address and client logon 
                ' credentials from the user.
                Dim serverConnect As New ServerConnection()
                Dim config As ServerConnection.Configuration = serverConnect.GetServerConfiguration()

                Dim app As New CRUDEmailAttachments()
                app.Run(config, True)

            Catch ex As FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault)
                Console.WriteLine("The application terminated with an error.")
                Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp)
                Console.WriteLine("Code: {0}", ex.Detail.ErrorCode)
                Console.WriteLine("Message: {0}", ex.Detail.Message)
                Console.WriteLine("Plugin Trace: {0}", ex.Detail.TraceText)
                Console.WriteLine("Inner Fault: {0}", If(Nothing Is ex.Detail.InnerFault, "No Inner Fault", "Has Inner Fault"))
            Catch ex As TimeoutException
                Console.WriteLine("The application terminated with an error.")
                Console.WriteLine("Message: {0}", ex.Message)
                Console.WriteLine("Stack Trace: {0}", ex.StackTrace)
                Console.WriteLine("Inner Fault: {0}", If(Nothing Is ex.InnerException.Message, "No Inner Fault", ex.InnerException.Message))
            Catch ex As Exception
                Console.WriteLine("The application terminated with an error.")
                Console.WriteLine(ex.Message)

                ' Display the details of the inner exception.
                If ex.InnerException IsNot Nothing Then
                    Console.WriteLine(ex.InnerException.Message)

                    Dim fe As FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault) = TryCast(ex.InnerException,  _
                        FaultException(Of Microsoft.Xrm.Sdk.OrganizationServiceFault))
                    If fe IsNot Nothing Then
                        Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp)
                        Console.WriteLine("Code: {0}", fe.Detail.ErrorCode)
                        Console.WriteLine("Message: {0}", fe.Detail.Message)
                        Console.WriteLine("Plugin Trace: {0}", fe.Detail.TraceText)
                        Console.WriteLine("Inner Fault: {0}", If(Nothing Is fe.Detail.InnerFault, "No Inner Fault", "Has Inner Fault"))
                    End If
                End If
            ' Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            ' SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

            Finally
                Console.WriteLine("Press <Enter> to exit.")
                Console.ReadLine()
            End Try

        End Sub
        #End Region ' Main

    End Class
End Namespace

' </snippetcrudemailattachments>