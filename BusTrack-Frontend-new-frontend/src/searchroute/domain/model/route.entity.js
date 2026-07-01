export class Route {
    constructor({id = '', name = '', companyId = null, estimatedTime = null, frequency = null} = {}) {
        this.id = id;
        this.name = name;
        this.companyId = companyId;
        this.estimatedTime = estimatedTime;
        this.frequency = frequency;
    }
}
