﻿import { Fetcher } from 'hr.fetcher';

/**
 * This interface strongly types the hal endpoint data.
 * @param {any} links
 */
interface HalData {
    _links: any,
    _embedded: any
}

/**
 * A single hal link how they appear in the collection.
 * @param {any} embeds
 */
export interface HalLink {
    href: string,
    method: string
}

/**
 * Info about a single hal link, will include the link's ref.
 * @param {any} embeds
 */
export interface HalLinkInfo {
    href: string,
    method: string,
    rel: string
}

export class Embed<T>{
    private name: string;
    private embeds: HalData[];
    private fetcher: Fetcher;

    constructor(name: string, embeds: HalData[], fetcher: Fetcher) {
        this.name = name;
        this.embeds = embeds;
        this.fetcher = fetcher;
    }

    public GetAllClients(): HalEndpointClient<T>[] {
        //No generators, create array
        var embeddedClients: HalEndpointClient<T>[] = [];

        for (let i = 0; i < this.embeds.length; ++i) {
            var embed = new HalEndpointClient<T>(this.embeds[i], this.fetcher);
            embeddedClients.push(embed);
        }
        return embeddedClients;
    }
}

/**
 * This class represents a single visit to a hal api endpoint. It will contain the data
 * that was requested and the links from that data.
 */
export class HalEndpointClient<T> {
    private static jsonMimeType = "application/json";

    /**
     * Load a hal link from an endpoint.
     * @param {HalLink} link - The link to load
     * @param {Fetcher} fetcher - The fetcher to use to load the link
     * @returns A HalEndpointClient for the link.
     */
    public static Load<T>(link: HalLink, fetcher: Fetcher): Promise<HalEndpointClient<T>> {
        return fetcher.fetch(link.href, {
            method: link.method
        })
        .then(r => HalEndpointClient.processResult<T>(r, fetcher));
    }

    private static processResult<T>(response: Response, fetcher: Fetcher): Promise<HalEndpointClient<T>> {
        return response.text().then((data) => {
            if (response.ok) {
                return new HalEndpointClient<T>(HalEndpointClient.parseResult(response, data), fetcher);
            }
            else {
                throw new Error("Error Code Returned. Make this error work better.");
            }
        });
    }

    private static parseResult(response: Response, data: string, jsonParseReviver?: (key: string, value: any) => any): HalData {
        var result: HalData;
        var contentHeader = response.headers.get('content-type');
        if (contentHeader && contentHeader.length >= HalEndpointClient.jsonMimeType.length && contentHeader.substring(0, HalEndpointClient.jsonMimeType.length) === HalEndpointClient.jsonMimeType) {
            result = data === "" ? null : JSON.parse(data, jsonParseReviver);
        }
        else {
            throw new Error("Unsupported response type " + contentHeader + ".");
        }
        return result;
    }

    private data: HalData; //This is our typed data, but as a hal object
    private fetcher: Fetcher;

    /**
     * Constructor.
     * @param {HalData} data - The raw hal data object.
     */
    constructor(data: HalData, fetcher: Fetcher) {
        this.data = data;
        this.fetcher = fetcher;
    }

    /**
     * Get the data portion of this client.
     * @returns The data.
     */
    public GetData(): T {
        //Tricky typecase, but HalDatas are also Ts in this instance.
        //Not here or relevant at runtime anyway.
        return <T><any>this.data;
    }

    /**
     * Get an embed.
     * @param {string} name - The name of the embed.
     * @returns - The embed specified by name or undefined.
     */
    public GetEmbed<T>(name: string): Embed<T> {
        return new Embed<T>(name, this.data._embedded[name], this.fetcher);
    }

    /**
     * See if this client has an embed.
     * @param {string} name - The name of the embed
     * @returns True if found, false otherwise.
     */
    public HasEmbed(name: string): boolean {
        return this.data._embedded[name] !== undefined;
    }

    /**
     * Get all the embeds in this client. If they are all the same type specify
     * T, otherwise use any to get generic objects.
     * @returns
     */
    public GetAllEmbeds<T>(): Embed<T>[] {
        //No generators, create array
        var embeds: Embed<T>[] = [];
        for (var key in this.data._embedded) {
            var embed = new Embed<T>(key, this.data._embedded[key], this.fetcher);
            embeds.push(embed);
        }
        return embeds;
    }

    /**
     * Load a new link, this will return a new HalEndpointClient for the results
     * of that request. You can keep using the client that you called this function
     * on to keep making requests if needed. The ref must exist before you can call
     * this function. Use HasLink to see if it is possible.
     * @param {string} ref - The link reference to visit.
     * @returns
     */
    public LoadLink<DataType>(ref: string): Promise<HalEndpointClient<DataType>> {
        if (this.HasLink(ref)) {
            return HalEndpointClient.Load<DataType>(this.GetLink(ref), this.fetcher);
        }
        else {
            throw new Error('Cannot find ref "' + ref + '".');
        }
    }

    /**
     * Get a single named link.
     * @param {string} ref - The name of the link to recover.
     * @returns The link or undefined if the link does not exist.
     */
    public GetLink(ref: string): HalLink {
        return this.data._links[ref];
    }

    /**
     * Check to see if a link exists in this collection.
     * @param {string} ref - The name of the link (the ref).
     * @returns - True if the link exists, false otherwise
     */
    public HasLink(ref: string): boolean {
        return this.data._links[ref] !== undefined;
    }

    /**
     * Get all links in this collection. Will transform them to a HalLinkInfo, these are copies of the original links with ref added.
     * @returns
     */
    public GetAllLinks(): HalLinkInfo[] {
        //If only we had generators, have to load entire collection
        var linkInfos: HalLinkInfo[] = [];
        for (var key in this.data._links) {
            var link: HalLink = this.data._links[key];
            linkInfos.push({
                href: link.href,
                method: link.method,
                rel: key
            });
        }
        return linkInfos;
    }
}