﻿import * as bootstrap from 'hr.bootstrap.all';
import * as HalEndpointClient from 'clientlibs.HalEndpointClient';
import * as WindowFetch from 'hr.windowfetch';
import * as uri from 'hr.uri';

interface Query {
    entry: string;
}

export class PageStart {
    private fetcher: WindowFetch.WindowFetch;
    private entryPoint: HalEndpointClient.HalEndpointClient<any>;

    constructor() {
        //Activate bootstrap here, this way we know its active and has registered all its models and toggles when starting up other controllers.
        bootstrap.activate();
        this.fetcher = new WindowFetch.WindowFetch();
    }

    /**
     * The entry point to the api.
     * @returns
     */
    get EntryPoint() {
        return this.entryPoint;
    }

    __loadResources(): Promise<PageStart> {
        var query: Query = <Query>uri.getQueryObject();
        if (query.entry === undefined) {
            query.entry = 'http://localhost:58151/api';
        }

        return HalEndpointClient.HalEndpointClient.Load({ href: query.entry, method: 'GET' }, this.fetcher)
            .then(client => this.entryPoint = client)
            .then(data => this);
    }
}

var instance: PageStart = null;
/**
 * Set up common config for the page to run.
 * @returns A Promise with the PageStart instance inside.
 */
export function init(): Promise<PageStart> {
    if (instance === null) {
        instance = new PageStart();
        return instance.__loadResources();
    }

    return Promise.resolve(instance);
}