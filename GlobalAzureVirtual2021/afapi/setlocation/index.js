module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');

    const name = (req.query.name || (req.body && req.body.name));
    const responseMessage = name
        ? "안녕하세요. " + name + "를(을) 희망 여행지로 등록했습니다."
        : status

    context.res = {
        // status: 200, /* Defaults to 200 */
        body: responseMessage
    };
}