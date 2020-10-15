import * as React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";

export default class Config extends React.Component<any, any> {

    public loaddata = () => {
        microsoftTeams.initialize();
        microsoftTeams.settings.registerOnSaveHandler((saveEvent) => {
            microsoftTeams.settings.setSettings({ websiteUrl: "https://faqplus-ternium.azurewebsites.net/", contentUrl: "https://faqplus-ternium.azurewebsites.net/myQuestions", entityId: "MyQuestions", suggestedDisplayName: "My questions" });
            saveEvent.notifySuccess();
        });
    
            }

    public render(): JSX.Element {
        return (<a href="#" onClick={()=>this.loaddata()}>hello config</a>);
        }
}