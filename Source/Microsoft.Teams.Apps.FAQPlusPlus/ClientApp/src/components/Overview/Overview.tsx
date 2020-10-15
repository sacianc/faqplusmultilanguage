import * as React from 'react';
import { initializeIcons } from 'office-ui-fabric-react/lib/Icons';
import { getBaseUrl } from '../../configVariables';
import * as microsoftTeams from "@microsoft/teams-js";
import { Loader, Input, Flex, Grid, Segment, FlexItem, Text, Dropdown, DropdownProps, Checkbox, Button } from '@fluentui/react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import { faSortUp, faSortDown, faTrashAlt } from '@fortawesome/free-solid-svg-icons';
import { AxiosResponse } from "axios";
import './Overview.scss';
import { orderBy } from 'lodash';
import { getAllTickets, deleteTicketDetails } from '../../apis/apiList';

declare var require: any
const sortImage = require(`../../images/sortImage.png`);


export interface ITicketsList {
    ticketId?: string;
    title: string;
    requesterName: string;
    requesterUserPrincipalName: string;
    status: number;
    assignedToName: string;
    assignedToObjectId: string;
    dateCreated: Date;
    description: string;
    answerBySME: string;
    isChecked: boolean;
    showDetails: boolean;
}

export interface ITicketsListProps {
    getAllTickets: () => Promise<AxiosResponse<ITicketsList[]>>;
}

export interface ITicketListsState {
    ticketLists: ITicketsList[];
    masterTicketsLists: ITicketsList[];
    trashIcon: IconDefinition,
    createdOnSortIcon: IconDefinition,
    loader: boolean;
    loggedUser: string;
    loggedUserObjId: string;
    selectedFilter: string;
    apiError: boolean;
}

export default class Overview extends React.Component<ITicketsListProps, ITicketListsState> {

    private historyArray: string[];
    private inputItems: string[];

    constructor(props: ITicketsListProps) {
        super(props);
        initializeIcons();
        this.escFunction = this.escFunction.bind(this);
        this.sortDataByColumn = this.sortDataByColumn.bind(this);
        this.historyArray = [];
        this.inputItems = [
            'All',
            'Unanswered',
            'Answered',
        ];
        this.state = {
            ticketLists: [], //Active display data. 
            masterTicketsLists: [], // master copy of ticket lists data
            loader: true,          //Indicates loader to display while data is loading
            trashIcon: faTrashAlt,
            createdOnSortIcon: faSortDown,
            loggedUser: '',
            loggedUserObjId: '',
            selectedFilter: 'All',
            apiError: false,
        };
    }

    public componentDidMount = () => {

        //Save Page URL to local storage to use for Back button in Tickets page
        const historyJson = localStorage.getItem("localStorageHistory");
        if (historyJson != null) {
            this.historyArray = JSON.parse(historyJson);
            if (this.historyArray.length > 0) {
                this.historyArray = [];
                this.historyArray.push(window.location.href);
                localStorage.setItem("localStorageHistory", JSON.stringify(this.historyArray));
            }
            else {
                this.historyArray.push(window.location.href);
                localStorage.setItem("localStorageHistory", JSON.stringify(this.historyArray));
            }
        }
        else {
            this.historyArray.push(window.location.href);
            localStorage.setItem("localStorageHistory", JSON.stringify(this.historyArray));
        }

        document.addEventListener("keydown", this.escFunction, false);
        this.dataLoad();
    }

    private dataLoad = () => {
        //To load data from server

        this.props.getAllTickets().then((response: AxiosResponse<ITicketsList[]>) => {

            const ticketLists: ITicketsList[] = JSON.parse(JSON.stringify(response.data));
            this.setState({
                ticketLists: orderBy(ticketLists, 'dateCreated', ["desc"]),
                masterTicketsLists: orderBy(ticketLists, 'dateCreated', ["desc"]),
                loader: false,
            })

            if (this.state.selectedFilter === 'Answered') {
                this.setState({
                    ticketLists: this.state.masterTicketsLists.filter((x: ITicketsList) => x.status === 1),
                })
            }
            else if (this.state.selectedFilter === 'Unanswered') {
                this.setState({
                    ticketLists: this.state.masterTicketsLists.filter((x: ITicketsList) => x.status === 0),
                })
            }
        });

        microsoftTeams.getContext(context => {
            this.setState({
                loggedUser: context.userPrincipalName ? context.userPrincipalName : "",
                loggedUserObjId: context.userObjectId ? context.userObjectId : "",
            })
        });
    }


    public componentWillUnmount = () => {
        document.removeEventListener("keydown", this.escFunction, false);
    }

    //#region "Sorting functions"

    //Calling appropriate function based on column selected for sorting
    private sortDataByColumn = (column: string, currentIcon: IconDefinition) => {
        let availableRecords = this.state.ticketLists;
        let newIcon = currentIcon;
        if (currentIcon === faSortDown) {
            availableRecords = orderBy(availableRecords, 'dateCreated', ["asc"]);
            newIcon = faSortUp;
        }
        else {
            availableRecords = orderBy(availableRecords, 'dateCreated', ["desc"]);
            newIcon = faSortDown;
        }

        this.setState({
            createdOnSortIcon: newIcon,
            ticketLists: availableRecords,
        })
    }

    //#endregion "Sorting functions"
    //#region "Search function"

    private searchTickets = (e: React.SyntheticEvent<HTMLElement, Event>) => {
        let searchQuery = (e.target as HTMLInputElement).value;
        if (!searchQuery) // If Search text cleared
        {
            this.setState({
                ticketLists: this.state.masterTicketsLists,
            })
        }
        else {
            this.setState({
                ticketLists: this.state.masterTicketsLists.filter((x: ITicketsList) => x.title.toLowerCase().includes(searchQuery.toLowerCase()) || x.ticketId?.includes(searchQuery)),
            })
        }
    }

    private filterTickets = (event: React.SyntheticEvent<HTMLElement>, data: any) => {
        let filterQuery = data.value;
        if (filterQuery === 'All') {
            this.setState({
                ticketLists: this.state.masterTicketsLists,
                selectedFilter: 'All',
            })
        }
        else if (filterQuery === 'Answered') {
            this.setState({
                ticketLists: this.state.masterTicketsLists.filter((x: ITicketsList) => x.status === 1),
                selectedFilter: 'Answered',
            })
        }
        else if (filterQuery === 'Unanswered') {
            this.setState({
                ticketLists: this.state.masterTicketsLists.filter((x: ITicketsList) => x.status === 0),
                selectedFilter: 'Unanswered',
            })
        }
    }
    //#endregion "Search function"

    //Handles escape function
    private escFunction = (e: KeyboardEvent) => {
        if (e.keyCode === 27 || (e.key === "Escape")) {
            microsoftTeams.tasks.submitTask();
        }
    }

    private toggleChecked = (e: any, data: any) => {
        let checkedValue = data.checked;
        const newTicketList = this.state.ticketLists.map((item) => {
            if (checkedValue) {
                const updatedItem = {
                    ...item,
                    isChecked: true,
                };

                return updatedItem;
            }
            else {
                const updatedItem = {
                    ...item,
                    isChecked: false,
                };

                return updatedItem;
            }

            return item;
        });

        this.setState({
            ticketLists: newTicketList,
        })
    }

    private toggleCheckedTicket = (ticketId: string, data: any) => {
        let checkedValue = data.checked;
        const newTicketList = this.state.ticketLists.map((item) => {
            if (item.ticketId == ticketId) {
                if (checkedValue) {
                    const updatedItem = {
                        ...item,
                        isChecked: true,
                    };

                    return updatedItem;
                }
                else {
                    const updatedItem = {
                        ...item,
                        isChecked: false,
                    };

                    return updatedItem;
                }
            }
            else {
                const updatedItem = {
                    ...item,
                };

                return updatedItem;
            }

            return item;
        });

        this.setState({
            ticketLists: newTicketList,
        })
    }

    private isDeleteBtnDisabled = () => {
        var ticketList = this.state.ticketLists.filter((x: ITicketsList) => x.isChecked === true);
        if (ticketList.length > 0) {
            return false;
        } else {
            return true;
        }
    }

    private onDelete = () => {
        let spanner = document.getElementsByClassName("savingLoader");
        spanner[0].classList.remove("hiddenLoader");
        let errorState = document.getElementsByClassName("errorMessage");
        errorState[0].classList.add("errorHidden");

        var ticketList = this.state.ticketLists.filter((x: ITicketsList) => x.isChecked === true);

        this.deleteSelected(ticketList).then(() => {
            if (this.state.apiError) {
                let spanner = document.getElementsByClassName("savingLoader");
                spanner[0].classList.add("hiddenLoader");
                let errorState = document.getElementsByClassName("errorMessage");
                errorState[0].classList.remove("errorHidden");
            } else {
                spanner[0].classList.add("hiddenLoader");
                this.dataLoad();
            }
        });
    }

    private deleteSelected = async (selectedTickets: {}) => {
        try {
            const response = await deleteTicketDetails(selectedTickets);
            this.setState({
                apiError: false,
            });
        } catch (error) {
            this.setState({
                apiError: true,
            });
            return error;
        }
    }

    public onShowAnswerDetails = (ticketId: string) => {
        const newTicketList = this.state.ticketLists.map((item) => {
            if (item.ticketId === ticketId) {
                const updatedItem = {
                    ...item,
                    showDetails: item.showDetails ? false : true,
                };

                return updatedItem;
            }
            else {
                const updatedItem = {
                    ...item,
                    showDetails: false,
                };

                return updatedItem;
            }

            return item;
        });

        this.setState({
            ticketLists: newTicketList,
        })
    }

    public onDeleteTicketDetails = (ticketId: string) => {
        var ticketList = this.state.ticketLists.filter((x: ITicketsList) => x.ticketId == ticketId);

        this.deleteSelected(ticketList).then(() => {
            if (this.state.apiError) {
                // Error, could not delete
            } else {
                this.dataLoad();
            }
        });
    }

    public render(): JSX.Element {
        //Page size drop down values.        
        let items = []; //Populate grid items
        let headeritems = []; //Populate grid items
        for (let j: number = 0; j < this.state.ticketLists.length; j++) {
            let createdDate = new Date(this.state.ticketLists[j].dateCreated);
            let locale = window.navigator.language;
            let localeDate = locale != null ? createdDate.toLocaleDateString(locale) : createdDate.toLocaleDateString();
            items.push(<Segment className="segmentStyle" styles={{ gridColumn: 'span 7', }}><div className="rowStyle"><div className="checkColumn"><Checkbox checked={this.state.ticketLists[j].isChecked == null ? false : this.state.ticketLists[j].isChecked} onChange={(e, data) => this.toggleCheckedTicket(this.state.ticketLists[j].ticketId || '0', data)} /></div>
                <div className="ticketIdColumn" onClick={() => this.onShowAnswerDetails(this.state.ticketLists[j].ticketId || '0')}>{this.state.ticketLists[j].ticketId}</div>
                <div className="titleColumn" onClick={() => this.onShowAnswerDetails(this.state.ticketLists[j].ticketId || '0')}>{this.state.ticketLists[j].title}</div>
                <div className="descriptionColumn" onClick={() => this.onShowAnswerDetails(this.state.ticketLists[j].ticketId || '0')}>{this.state.ticketLists[j].description}</div>
                <div className="dateColumn" onClick={() => this.onShowAnswerDetails(this.state.ticketLists[j].ticketId || '0')}>{localeDate}</div>
                <div className="statusColumn" onClick={() => this.onShowAnswerDetails(this.state.ticketLists[j].ticketId || '0')}>
                    <Text content={this.state.ticketLists[j].status === 0 ? 'Unanswered' : 'Answered'} />
                </div>
                <div className="deleteColumn trashIconStyle" ><FontAwesomeIcon icon={this.state.trashIcon} onClick={() => this.onDeleteTicketDetails(this.state.ticketLists[j].ticketId || '0')} /></div>
            </div></Segment>)
            if (this.state.ticketLists[j].showDetails) {
                items.push(<Segment className="pointer" styles={{ gridColumn: 'span 7', wrap: true, boxAlign: "center" }} >
                    <div className="AnswerbySMEHeaderText" ><Text content={'Answer by Expert'} /></div><div className="AnswerbySMEText"><Text content={this.state.ticketLists[j].answerBySME != null ? this.state.ticketLists[j].answerBySME : 'Not yet answered'} /></div></Segment>)
            }
        }

        let segmentRows = []; //Populate grid 
        if (this.state.loader) {
            segmentRows.push(<Segment styles={{ gridColumn: 'span 7', }}>< Loader /></Segment >);
        }
        else {
            segmentRows.push(items);
        }

        headeritems.push(<Segment className="segmentHeaderStyle" styles={{ gridColumn: 'span 7', }}><div className="headerRowStyle">
            <div className="checkColumn backGroundColor headerStyle"><Checkbox onChange={this.toggleChecked} /></div>
            <div className="ticketIdColumn backGroundColor headerStyle" ><Text content="Ticket ID" /></div>
            <div className="titleColumn backGroundColor headerStyle"><Text content="Question title" /></div>
            <div className="descriptionColumn backGroundColor headerStyle" ><Text content="Description" /></div>
            <div className="dateColumn backGroundColor headerStyle" >
                <Flex gap="gap.small">
                    <FlexItem grow>
                        <Text content="Created on" />
                    </FlexItem>
                    <FlexItem push>
                        <img src={sortImage} className="imgStyle" onClick={() => this.sortDataByColumn('dateCreated', this.state.createdOnSortIcon)} />
                    </FlexItem>
                </Flex>
            </div>
            <div className="statusColumn backGroundColor headerStyle" ><Text content="Status" /></div>
            <div className="deleteColumn backGroundColor headerStyle" ><Text content="" /></div>
        </div></Segment>
        )

        return (
            <div className="mainComponent backGroundColor">
                <div className={"formContainer"}>
                    <Flex space="between" >
                        <FlexItem grow>
                            <Text content="Pending actions" size={"medium"} weight="bold" className="textstyle" />
                        </FlexItem >

                        <div className="paddingRight">
                            <Flex gap="gap.medium">
                                <Dropdown
                                    className="bold pointer width_dropdown"
                                    aria-label="Filter"
                                    items={this.inputItems}
                                    defaultValue={this.inputItems[0]}
                                    placeholder="Filter"
                                    checkable
                                    onChange={this.filterTickets}
                                    fluid
                                />
                                <Input aria-label="Search" icon="search" placeholder="Search by ticket id, question title" onChange={this.searchTickets} className="textstyle whiteColor width_Search" />
                            </Flex>
                        </div>
                    </Flex>

                    <div className="headerRow">
                        <Grid columns="0.5fr 1.3fr 2.3fr 3.5fr 1.3fr 1.3fr 0.5fr" >
                            {headeritems}
                        </Grid>
                    </div>
                    <div className="formContentContainer" >
                        
                        <Grid columns="0.5fr 1.3fr 2.3fr 3.3fr 1.3fr 1.3fr 0.5fr" >
                            {segmentRows}

                        </Grid>
                    </div>
                </div>
                <div className={"footerContainer backGroundColor marginZero"}>
                    <div className="buttonContainer">
                        <Loader id="savingLoader" className="hiddenLoader savingLoader" size="smallest" label="Deleting tickets" labelPosition="end" />
                        <Text content="Sorry, an error occurred. Please try again." className="errorMessage errorHidden" error size="medium" />
                        <Button content="Delete selected" disabled={this.isDeleteBtnDisabled()} id="saveBtn" onClick={this.onDelete} primary />
                    </div>
                </div>
            </div>
        );
    }
}