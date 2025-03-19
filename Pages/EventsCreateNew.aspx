<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EventsCreateNew.aspx.cs" Inherits="SML.Pages.EventsCreateNew" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" type="text/css" href="/Content/Event.css" />
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="events-create-new">

                
        <section class="new-event-name-container">
            <asp:Label id="eventErrorLabel" CssClass="label-red" runat="server"></asp:Label>
            <asp:Label id="nameLabel" CssClass="label-white" runat="server">New Event Name</asp:Label>
            <asp:Panel ID="eventNamePanel" CssClass="panel" MaxLength="50" runat="server" DefaultButton="buttonContinue">
                <asp:TextBox id="nameTextbox" CssClass="textbox" runat="server"></asp:TextBox>
                <asp:Button ID="buttonContinue" Style="display:none;" OnClick="Submit_EventName" runat="server" />
            </asp:Panel>
            <asp:Panel ID="eventPasswordPanel" CssClass="panel" Visible="false" runat="server" DefaultButton="buttonContinue2">
                <asp:TextBox id="passwordTextbox" CssClass="textbox" MaxLength="50" runat="server"></asp:TextBox>
                <asp:Button ID="buttonContinue2" Style="display:none;" OnClick="Submit_EventPassword" runat="server" />
            </asp:Panel>
            <asp:Label ID="charErrorLabel" runat="server" CssClass="label-red"></asp:Label>

        </section>

        <script>
            document.addEventListener("DOMContentLoaded", function () {
                var textboxes = [
                    { input: "<%= nameTextbox.ClientID %>", error: "<%= charErrorLabel.ClientID %>" },
                    { input: "<%= passwordTextbox.ClientID %>", error: "<%= charErrorLabel.ClientID %>" }
            ];

            textboxes.forEach(function (item) {
                var textbox = document.getElementById(item.input);
                var errorLabel = document.getElementById(item.error);

                if (textbox) {
                    textbox.addEventListener("input", function () {
                        var regex = /^[a-zA-Z0-9\s]{1,50}$/;
                        if (!regex.test(textbox.value)) {
                            errorLabel.innerText = "Only letters and numbers are allowed. Max 50 characters.";
                            textbox.value = textbox.value.replace(/[^a-zA-Z0-9\s]/g, "").substring(0, 50);
                        } else {
                            errorLabel.innerText = "";
                        }
                    });
                }
            });
        });
        </script>


    </main>
</asp:Content>
