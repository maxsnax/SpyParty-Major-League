<%@ Page Title="Upload" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Upload.aspx.cs" Inherits="SML.Upload" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <script>
        function toggleTextVisibility(checkbox) {
            var tooltipText = checkbox.parentElement.querySelector('.tooltiptext');
            var isVisible = tooltipText.getAttribute('data-visible') === 'true';

            // Toggle visibility
            isVisible = !isVisible;
            tooltipText.style.display = isVisible ? 'block' : 'none';
            tooltipText.setAttribute('data-visible', isVisible.toString());
        }
    </script>

    <main aria-labelledby="title">
        
        <section class="upload-row" aria-labelledby="fileUploadOuterContainer" >

        <img src="Images/divisions/SML-Logo.png" height="200"/>
        <h2 id="title">Upload SML Replays</h2>
            <section class="row" aria-labelledby="fileUploadRow">
                <p>Provide a single .zip file containing all of the replays for a single match.</p>
                <asp:FileUpload ID="fileUploadData" runat="server" Width="1000"></asp:FileUpload>
                <div class="button-container" style="padding-top: 10px">
                    <asp:DropDownList ID="selectSeasonList" runat="server"/>
                    <asp:Button ID="uploadFileButton" runat="server" Text="Upload file" OnClick="uploadFileButton_Click" Width="205px" />
                    
                </div>
                <asp:Label ID="fileUploadNameLabel" runat="server" ></asp:Label>
            </section>
        </section>

        <section class="file-table-row" aria-labelledby="fileUploadOuterContainer">

        <table style="white-space:pre-wrap; word-wrap:break-word;" id="fileTable" runat="server" class="table">
            <thead>
                <tr>
                    <th>File Name</th>
                    <th>Upload Date</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
            <!-- Table rows will be generated dynamically -->
            </tbody>
        </table>
            <!-- <asp:Label ID="fileData" Text="Test" runat="server" ></asp:Label><p> -->
        </section>

        <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
        <asp:PlaceHolder runat="server" ID="masterTablePlaceholder"></asp:PlaceHolder>

    </main>
</asp:Content>
