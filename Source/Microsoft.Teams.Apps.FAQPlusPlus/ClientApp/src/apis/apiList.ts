import axios from './axiosJWTDecorator';
import { getBaseUrl } from '../configVariables';
import { AxiosResponse } from "axios";
import { ITicketsList } from "./../components/Overview/Overview";

let baseAxiosUrl = getBaseUrl() + '/api';

export const getAllTickets = async (): Promise<AxiosResponse<ITicketsList[]>> => {
    let url = baseAxiosUrl + "/tickets";
    return await axios.get(url);
}

export const deleteTicketDetails = async (payload: {}): Promise<any> => {
    let url = baseAxiosUrl + "/tickets/deleteTicketDetails";
    return await axios.post(url, payload);
}

export const getAuthenticationMetadata = async (windowLocationOriginDomain: string, loginHint: string): Promise<AxiosResponse<string>> => {
    let url = `${baseAxiosUrl}/authenticationMetadata/GetAuthenticationUrlWithConfiguration?windowLocationOriginDomain=${windowLocationOriginDomain}&loginhint=${loginHint}`;
    return await axios.get(url, undefined, false);
}
